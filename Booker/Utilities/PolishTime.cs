namespace Booker.Utilities;

/// <summary>
/// Single source for rendering stored UTC times in the Polish wall clock.
/// Storage keeps UTC and EF materializes it with Kind=Unspecified, so every
/// conversion must state its source zone explicitly.
/// </summary>
public static class PolishTime
{
    private static readonly TimeZoneInfo Warsaw = CreateWarsawZone();

    public static DateTime ToLocal(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Warsaw);

    private static TimeZoneInfo CreateWarsawZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");
        }
        // Without ICU, Windows only knows its own zone id.
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        }
    }
}
