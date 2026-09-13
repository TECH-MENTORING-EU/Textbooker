namespace Booker.Services;

// RODO - Phase 1: Configuration for guardian consent process.
// Token expiration and cleanup interval for unconfirmed minor accounts.
public class GuardianConsentOptions
{
    public int TokenExpirationDays { get; set; } = 7;
    public int CleanupIntervalMinutes { get; set; } = 60;
    public int ConfirmedDataRetentionDays { get; set; } = 90;
}
