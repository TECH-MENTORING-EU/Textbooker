using System.Text.RegularExpressions;

namespace Booker.Pages.Shared;

// RODO - task 08: a simple, deliberately imperfect heuristic for detecting contact details
// in a listing's description. Does not block publishing - only triggers a confirmation warning.
public static class ContentModerationHelper
{
    private static readonly Regex EmailPattern = new(
        @"[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}",
        RegexOptions.Compiled);

    // Nine digits in a row, allowing spaces/hyphens between groups and an optional +48 prefix -
    // matches typical formatting of a Polish mobile phone number.
    private static readonly Regex PhonePattern = new(
        @"(\+?48[\s\-]?)?(\d[\s\-]?){9}",
        RegexOptions.Compiled);

    public static bool LooksLikeContactInfo(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return EmailPattern.IsMatch(text) || PhonePattern.IsMatch(text);
    }
}
