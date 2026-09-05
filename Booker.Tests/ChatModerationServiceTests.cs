using Booker.Services;
using Xunit;

namespace Booker.Tests;

/// <summary>
/// Chat anti-spam gate: sliding-window rate limit (10 msg/min), duplicate
/// suppression, link blocking, banned word filtering.
/// </summary>
public class ChatModerationServiceTests
{
    private static readonly string[] TestBannedWords = ["kurwa", "nigga", "хуй"];

    private static (ChatModerationService Svc, DateTimeOffset T0) New() =>
        (new ChatModerationService(TestBannedWords), new DateTimeOffset(2026, 9, 3, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public void AcceptsNormalMessage()
    {
        var (svc, t0) = New();

        Assert.Equal(ChatModerationService.ModerationVerdict.Accepted,
            svc.Check(1, "Cześć, książka jeszcze dostępna?", t0));
    }

    [Fact]
    public void BlocksEleventhMessageWithinWindow()
    {
        var (svc, t0) = New();

        for (var i = 0; i < 10; i++)
        {
            Assert.Equal(ChatModerationService.ModerationVerdict.Accepted,
                svc.Check(1, $"Message {i}", t0.AddSeconds(i)));
        }

        Assert.Equal(ChatModerationService.ModerationVerdict.RateLimited,
            svc.Check(1, "Message 10", t0.AddSeconds(10)));
    }

    [Fact]
    public void WindowSlides_MessageAllowedAgainAfter60Seconds()
    {
        var (svc, t0) = New();

        for (var i = 0; i < 10; i++)
        {
            svc.Check(1, $"Message {i}", t0.AddSeconds(i));
        }

        Assert.Equal(ChatModerationService.ModerationVerdict.RateLimited,
            svc.Check(1, "too soon", t0.AddSeconds(30)));

        // oldest message left the window
        Assert.Equal(ChatModerationService.ModerationVerdict.Accepted,
            svc.Check(1, "later is fine", t0.AddSeconds(61)));
    }

    [Fact]
    public void RateLimitIsPerUser()
    {
        var (svc, t0) = New();

        for (var i = 0; i < 10; i++)
        {
            svc.Check(1, $"Message {i}", t0.AddSeconds(i));
        }

        Assert.Equal(ChatModerationService.ModerationVerdict.Accepted,
            svc.Check(2, "other user is unaffected", t0.AddSeconds(5)));
    }

    [Fact]
    public void BlocksImmediateDuplicate()
    {
        var (svc, t0) = New();

        Assert.Equal(ChatModerationService.ModerationVerdict.Accepted, svc.Check(1, "ok", t0));
        Assert.Equal(ChatModerationService.ModerationVerdict.Duplicate, svc.Check(1, "ok", t0.AddSeconds(1)));
    }

    [Fact]
    public void DuplicateCheckIsCaseAndWhitespaceInsensitive()
    {
        var (svc, t0) = New();

        Assert.Equal(ChatModerationService.ModerationVerdict.Accepted, svc.Check(1, "Do zamka!", t0));
        Assert.Equal(ChatModerationService.ModerationVerdict.Duplicate, svc.Check(1, "  do zamka!  ", t0.AddSeconds(1)));
    }

    [Theory]
    [InlineData("Kup tanio na https://scam.example.de/okazja")]
    [InlineData("See www.aliexpress.com deal")]
    [InlineData("Napisz do mnie: kupujacy123@interia.pl x www.onet.pl")]
    [InlineData("link bez protokołu: scam.xyz/branie")]
    public void BlocksLinksWithAndWithoutProtocol(string content)
    {
        var (svc, t0) = New();

        Assert.Equal(ChatModerationService.ModerationVerdict.LinkBlocked, svc.Check(1, content, t0));
    }

    [Fact]
    public void AcceptsPlainTextWithDotsAndNumbers()
    {
        var (svc, t0) = New();

        Assert.Equal(ChatModerationService.ModerationVerdict.Accepted,
            svc.Check(1, "Mam 3. wydanie, stan dobry, cena 45 zl do negocjacji", t0));
    }

    [Theory]
    [InlineData("nigga")]
    [InlineData("NIGGA")]
    [InlineData("no co ty nigga")]
    [InlineData("k.u.r.w.a!")]
    [InlineData("kurw a")]
    public void BlocksBannedWordsIncludingObfuscatedOnes(string content)
    {
        var (svc, t0) = New();

        Assert.Equal(ChatModerationService.ModerationVerdict.ProfanityBlocked, svc.Check(1, content, t0));
    }

    [Theory]
    [InlineData("хуй тебе")]
    [InlineData("Х У Й")]
    [InlineData("ти хуй, а не продавець")]
    public void BlocksCyrillicBannedWords(string content)
    {
        var (svc, t0) = New();

        Assert.Equal(ChatModerationService.ModerationVerdict.ProfanityBlocked, svc.Check(1, content, t0));
    }

    [Fact]
    public void AcceptsWordsThatMerelyLookSimilar()
    {
        var (svc, t0) = New();

        Assert.Equal(ChatModerationService.ModerationVerdict.Accepted,
            svc.Check(1, "napisze ci wieczorem", t0));
    }

    [Theory]
    [InlineData("Привіт! Книга ще доступна?")]
    [InlineData("Доброго дня, скільки коштує доставка?")]
    [InlineData("Дякую, забираю завтра о 15:00")]
    [InlineData("Mogę zapłacić BLIK-iem jutko rano")]
    [InlineData("Pierogi i kurczak to moje ulubione dania")]
    public void AcceptsUkrainianAndSimilarLookingMessages(string content)
    {
        var (svc, t0) = New();

        Assert.Equal(ChatModerationService.ModerationVerdict.Accepted, svc.Check(1, content, t0));
    }

    [Fact]
    public void CyrillicWordBoundaryKeepsSimilarWordsAllowed()
    {
        // "бля" is banned, but "бляха" (a real word) must not be caught by
        // the same rule - the boundary check works for Cyrillic too.
        var svc = new ChatModerationService(["бля"]);
        var t0 = DateTimeOffset.UtcNow;

        Assert.Equal(ChatModerationService.ModerationVerdict.ProfanityBlocked, svc.Check(1, "бля ти", t0));
        Assert.Equal(ChatModerationService.ModerationVerdict.Accepted, svc.Check(1, "бляха м'яка", t0));
    }

    [Fact]
    public void EmptyWordListDisablesTheFilter()
    {
        var svc = new ChatModerationService(Array.Empty<string>());

        Assert.Equal(ChatModerationService.ModerationVerdict.Accepted,
            svc.Check(1, "kurwa", DateTimeOffset.UtcNow));
    }
}
