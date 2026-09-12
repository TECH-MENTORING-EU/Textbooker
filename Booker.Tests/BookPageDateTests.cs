using Booker.Pages;
using Xunit;

namespace Booker.Tests;

/// <summary>
/// Date rendering on the offer page after the UtcNow migration: storage is UTC,
/// so "dzisiaj"/"wczoraj" must be evaluated in Polish local time, not server time.
/// Every case pins the Polish-time judgement with an explicit clock.
/// </summary>
public class BookPageDateTests
{
    private static readonly TimeZoneInfo Warsaw = CreateWarsawZone();

    private static TimeZoneInfo CreateWarsawZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");
        }
        // Without ICU, Windows only knows its own zone id (same fallback as BookModel).
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        }
    }

    [Fact]
    public void FormatDateWithSpecialCases_Null_ReturnsBrakDaty()
    {
        Assert.Equal("Brak daty", BookModel.FormatDateWithSpecialCases(null));
    }

    [Fact]
    public void FormatDateWithSpecialCases_VeryOldDate_DoesNotSayDzisiaj()
    {
        var nowUtc = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        var result = BookModel.FormatDateWithSpecialCases(
            new DateTime(2025, 12, 16, 12, 0, 0, DateTimeKind.Utc), nowUtc, Warsaw);

        Assert.DoesNotContain("dzisiaj", result);
        // A month-old offer falls through to the plain Polish date format.
        Assert.Equal("16 grudnia o 13:00", result);
    }

    [Fact]
    public void FormatDateWithSpecialCases_RecentDate_UsesSpecialCase()
    {
        // Around Polish midnight the special cases flip on the Warsaw calendar,
        // not on UTC: offers seven real minutes apart land on different sides.
        var nowUtc = new DateTime(2026, 1, 16, 0, 5, 0, DateTimeKind.Utc);

        var justAfterMidnight = BookModel.FormatDateWithSpecialCases(
            new DateTime(2026, 1, 15, 23, 58, 0, DateTimeKind.Utc), nowUtc, Warsaw);
        var justBeforeMidnight = BookModel.FormatDateWithSpecialCases(
            new DateTime(2026, 1, 15, 22, 57, 0, DateTimeKind.Utc), nowUtc, Warsaw);

        Assert.Equal("dzisiaj o 00:58", justAfterMidnight);
        Assert.Equal("wczoraj o 23:57", justBeforeMidnight);
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
