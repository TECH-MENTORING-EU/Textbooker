using System;

namespace Booker.Utilities;

public static class StringExtensions
{
    public static int IntOrDefault(this string? str)
    {
        if (!int.TryParse(str, out int value))
        {
            return -1;
        }
        return value;
    }
}
