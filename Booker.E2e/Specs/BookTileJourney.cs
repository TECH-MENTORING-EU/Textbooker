using Microsoft.Playwright;

namespace Booker.E2e.Specs;

/// <summary>
/// The book tile cover regression (769ef06, issue #56): the cover image must be
/// a link to the detail page - clicking the picture navigates, not just the
/// title.
/// </summary>
[Collection("E2E")]
public class BookTileJourney(E2eWebAppFixture fixture)
{
    [Fact]
    public async Task The_cover_image_is_a_link_into_the_detail_page()
    {
        var page = await Browsers.Shared.NewPageAsync(fixture.BaseUrl + "/Browse");

        var cover = page.Locator(".book-tile a.cover").First;
        await Assertions.Expect(cover.Locator("img")).ToBeVisibleAsync();
        var href = await cover.GetAttributeAsync("href");
        Assert.Matches("^/Book/\\d+$", href!);
    }

    [Fact]
    public async Task Clicking_the_cover_lands_on_the_detail_page()
    {
        var page = await Browsers.Shared.NewPageAsync(fixture.BaseUrl + "/Browse");

        var href = await page.Locator(".book-tile a.cover").First.GetAttributeAsync("href");
        await page.Locator(".book-tile a.cover").First.ClickAsync();
        await page.WaitForURLAsync(u => new Uri(u).AbsolutePath.StartsWith("/Book/"));

        // The detail page of that very tile renders with a pl-PL price.
        Assert.Equal(href, new Uri(page.Url).PathAndQuery);
        var body = await page.TextContentAsync("body");
        Assert.Contains("zł", body);
    }
}
