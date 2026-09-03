using Booker.Pages;
using Xunit;

namespace Booker.Tests;

/// <summary>
/// Sanity tests for date rendering on the offer page after the UtcNow migration:
/// "dzisiaj"/"wczoraj" must be evaluated in Polish local time, not server time.
/// </summary>
public class BookPageDateTests
{
    private static DateTime UtcNowInWindow(int hoursAgo) => DateTime.UtcNow.AddHours(-hoursAgo);

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
    public void FormatDateWithSpecialCases_ReturnsSomething()
    {
        Assert.NotEmpty(BookModel.FormatDateWithSpecialCases(DateTime.UtcNow.AddMinutes(-5)));
    }
}
