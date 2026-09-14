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

    // Bare storage keys are plain relative object names (e.g. "guid.jpeg").
    // A leading slash, a backslash anywhere, or a colon anywhere marks a rooted
    // path, network-path reference, or absolute URL — none of these may be
    // appended to a base URL.
    public static bool IsBareStorageKey(this string value)
    {
        return !(value.StartsWith('/') || value.StartsWith('\\')
            || value.Contains(':') || value.Contains('\\'));
    }
}
