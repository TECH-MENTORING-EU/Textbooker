namespace Booker.Services;

// RODO — zadanie 07: hojne, ale skończone limity ujawniania danych kontaktowych sprzedającego.
// Liczone per konto, w pamięci procesu (patrz ContactRevealLimiter) — każde ujawnienie się liczy,
// również powtórne obejrzenie kontaktu do tego samego ogłoszenia.
public class ContactRevealLimitOptions
{
    public int PerHour { get; set; } = 60;
    public int PerDay { get; set; } = 200;
}
