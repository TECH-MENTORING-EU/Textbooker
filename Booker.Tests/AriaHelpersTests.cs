using Booker.Utilities;
using Xunit;

namespace Booker.Tests;

public class AriaHelpersTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CurrentOrNull_NullOrWhitespace_ReturnsNull(string? input)
    {
        Assert.Null(AriaHelpers.CurrentOrNull(input));
    }

    [Theory]
    [InlineData("page", "page")]
    [InlineData("step", "step")]
    [InlineData("location", "location")]
    public void CurrentOrNull_NonEmpty_ReturnsValue(string input, string expected)
    {
        Assert.Equal(expected, AriaHelpers.CurrentOrNull(input));
    }
}
