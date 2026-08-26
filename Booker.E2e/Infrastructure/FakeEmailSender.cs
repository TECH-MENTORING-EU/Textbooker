using Microsoft.AspNetCore.Identity.UI.Services;

namespace Booker.E2e.Infrastructure;

/// <summary>
/// Replaces SendMailSvc in the test host: records every message instead of talking to SMTP,
/// so tests can assert on confirmation links without external services.
/// </summary>
public sealed class FakeEmailSender : IEmailSender
{
    public List<(string Email, string Subject, string Body)> Sent { get; } = new();

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        Sent.Add((email, subject, htmlMessage));
        return Task.CompletedTask;
    }
}
