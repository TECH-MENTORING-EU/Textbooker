namespace Booker.Utilities;

/// <summary>
/// Minimal in-process retry helper for transient failures (e.g. SMTP, external HTTP calls).
/// Retries a boolean-returning operation with a linearly increasing delay between attempts.
/// </summary>
public static class RetryHelper
{
    public static async Task<bool> TryWithRetryAsync(
        Func<Task<bool>> operation,
        int maxAttempts,
        TimeSpan delayStep)
    {
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            if (await operation())
            {
                return true;
            }

            if (attempt < maxAttempts)
            {
                await Task.Delay(delayStep * attempt);
            }
        }

        return false;
    }
}
