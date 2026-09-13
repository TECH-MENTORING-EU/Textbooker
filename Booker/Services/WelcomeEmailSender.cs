using Booker.Utilities;

namespace Booker.Services
{
    /// <summary>
    /// Sends the one-time welcome/activation email with a small in-process retry so a
    /// transient SMTP failure does not silently drop the only notification the user gets.
    /// Callers must first claim the send via <see cref="GuardianConsentService.TryClaimWelcomeEmailAsync"/>
    /// (or an equivalent idempotency marker) so the two activation paths - student email
    /// confirmation and guardian consent confirmation - can never send it twice.
    /// </summary>
    public static class WelcomeEmailSender
    {
        private const int MaxAttempts = 3;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

        public static async Task SendWithRetryAsync(SendMailSvc mailSvc, ILogger logger, string email)
        {
            if (await RetryHelper.TryWithRetryAsync(
                () => mailSvc.TrySendEmailAsync(email, WelcomeEmailTemplate.Subject, WelcomeEmailTemplate.Body),
                MaxAttempts,
                RetryDelay))
            {
                return;
            }

            // The idempotency marker is already persisted at this point, so we won't
            // automatically retry again later - but we log at Error/Critical level so
            // an operator can notice and resend manually if needed.
            logger.LogError(
                "Failed to send welcome email to {Email} after {Attempts} attempts. The activation marker is already set, so this email will not be retried automatically.",
                email,
                MaxAttempts);
        }
    }
}
