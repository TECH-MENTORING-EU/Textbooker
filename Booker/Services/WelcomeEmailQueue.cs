using System.Threading.Channels;

namespace Booker.Services;

/// <summary>
/// Thread-safe hand-off point between the activation request and the background sender.
/// The welcome-email send itself (up to 3 SMTP attempts with 2s/4s backoff) is too slow
/// to run inline on the activation GET/POST, so callers queue the email here right after
/// GuardianConsentService.TryClaimWelcomeEmailAsync has already made the send idempotent.
/// </summary>
public interface IWelcomeEmailQueue
{
    void QueueWelcomeEmail(string email);
}

public class WelcomeEmailQueue : IWelcomeEmailQueue
{
    private readonly Channel<string> _channel = Channel.CreateUnbounded<string>();

    public ChannelReader<string> Reader => _channel.Reader;

    public void QueueWelcomeEmail(string email)
    {
        _channel.Writer.TryWrite(email);
    }
}
