using System.Text.RegularExpressions;

namespace Booker.Pages.Shared;

// RODO — zadanie 08: prosta, celowo niedoskonała heurystyka wykrywania danych kontaktowych
// w opisie ogłoszenia. Nie blokuje publikacji — tylko włącza ostrzeżenie z prośbą o potwierdzenie.
public static class ContentModerationHelper
{
    private static readonly Regex EmailPattern = new(
        @"[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}",
        RegexOptions.Compiled);

    // Dziewięć cyfr z rzędu, dopuszczając spacje/myślniki między grupami i opcjonalny prefiks +48 —
    // pasuje do typowych zapisów polskiego numeru telefonu komórkowego.
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
