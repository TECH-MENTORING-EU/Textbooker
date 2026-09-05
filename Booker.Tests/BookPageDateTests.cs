using Booker.Pages;
using Xunit;

namespace Booker.Tests;

/// <summary>
/// Date rendering on the offer page after the UtcNow migration: storage is UTC,
/// so "dzisiaj"/"wczoraj" must be evaluated in Polish local time, not server time.
/// The deterministic cases pin the Polish-time judgement with an explicit clock.
/// </summary>
public class BookPageDateTests
{
    private static readonly TimeZoneInfo Warsaw =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");

    [Fact]
    public void FormatDateWithSpecialCases_Null_ReturnsBrakDaty()
    {
        Assert.Equal("Brak daty", BookModel.FormatDateWithSpecialCases(null));
    }

    [Fact]
    public void FormatDateWithSpecialCases_VeryOldDate_DoesNotSayDzisiaj()
    {
        var old = DateTime.UtcNow.AddDays(-30);

        var result = BookModel.FormatDateWithSpecialCases(old);

        Assert.DoesNotContain("dzisiaj", result);
    }

    [Fact]
    public void FormatDateWithSpecialCases_RecentDate_UsesSpecialCase()
    {
        // Five minutes ago is at worst "wczoraj" (Polish midnight just passed),
        // never the plain date format reserved for older dates.
        var result = BookModel.FormatDateWithSpecialCases(DateTime.UtcNow.AddMinutes(-5));

        Assert.Matches(@"^(dzisiaj|wczoraj) o \d{2}:\d{2}$", result);
    }

    [Fact]
    public void FormatDateWithSpecialCases_PolishMidnightBoundary_UsesWarsawDate()
    {
        // 2026-01-15 23:30 UTC is already 2026-01-16 00:30 in Warsaw.
        var nowUtc = new DateTime(2026, 1, 15, 23, 30, 0, DateTimeKind.Utc);

        var samePolishDay = BookModel.FormatDateWithSpecialCases(
            new DateTime(2026, 1, 15, 23, 0, 0, DateTimeKind.Utc), nowUtc, Warsaw);

        Assert.StartsWith("dzisiaj o", samePolishDay);
    }

    [Fact]
    public void FormatDateWithSpecialCases_DayBeforeInWarsaw_SaysWczoraj()
    {
        // 2026-01-15 22:30 UTC is 2026-01-15 23:30 in Warsaw - the day before "now".
        var nowUtc = new DateTime(2026, 1, 15, 23, 30, 0, DateTimeKind.Utc);

        var previousPolishDay = BookModel.FormatDateWithSpecialCases(
            new DateTime(2026, 1, 15, 22, 30, 0, DateTimeKind.Utc), nowUtc, Warsaw);

        Assert.StartsWith("wczoraj o", previousPolishDay);
    }

    [Fact]
    public void FormatDateWithSpecialCases_RendersPolishWallClockTime()
    {
        var nowUtc = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        var result = BookModel.FormatDateWithSpecialCases(
            new DateTime(2026, 1, 15, 11, 30, 0, DateTimeKind.Utc), nowUtc, Warsaw);

        // 11:30 UTC is 12:30 CET; the wall-clock time shown must be Polish, not UTC.
        Assert.Equal("dzisiaj o 12:30", result);
    }
}
