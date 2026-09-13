using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Booker.Services
{
    public class SendMailSvc : IEmailSender
    {
        SmtpSettings _smtpSettings;
        ILogger<SendMailSvc> _log;

      
        public SendMailSvc(ILogger<SendMailSvc> logger, IOptions<SmtpSettings> smtpSettings)
        {
            _log = logger;
            _smtpSettings = smtpSettings.Value;
        }

        // Lets exceptions propagate; both public methods below decide what to do with them
        // (swallow-and-log, or report failure to the caller) so that decision isn't
        // duplicated at every call site across the app.
        private async Task SendCoreAsync(string email, string subject, string htmlMessage)
        {
            using SmtpClient smtpClient = new SmtpClient();

            smtpClient.Host = _smtpSettings.Server;
            smtpClient.Port = _smtpSettings.Port;
            smtpClient.EnableSsl = _smtpSettings.EnableSsl;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);

            var message = new MailMessage();
            message.Subject = subject;
            message.Body = htmlMessage;
            message.To.Add(email);
            message.From = new MailAddress("no-reply@textbooker.pl");
            message.IsBodyHtml = true;

            await smtpClient.SendMailAsync(message);
        }

        // IEmailSender implementation: never throws. Most callers just want best-effort
        // delivery and would otherwise all need their own try/catch to avoid turning a
        // transient SMTP failure into a 500 (or leaking delivery status via an
        // anti-enumeration response). A failure is still logged here.
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                await SendCoreAsync(email, subject, htmlMessage);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to send email to {Email} with subject '{Subject}'.", email, subject);
            }
        }

        // For the rare caller that must react to delivery failure (e.g. skip rotating a
        // stored token when the replacement email could not be delivered).
        public async Task<bool> TrySendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                await SendCoreAsync(email, subject, htmlMessage);
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to send email to {Email} with subject '{Subject}'.", email, subject);
                return false;
            }
        }

        public class SmtpSettings
        {
            public required string Server { get; set; }
            public required int Port { get; set; }
            public required string Username { get; set; }
            public required string Password { get; set; }

            /// <summary>
            /// STARTTLS on the standard submission port. Defaults to true so a
            /// missing config entry cannot silently downgrade mail to plaintext;
            /// set to false only for local relays without TLS.
            /// </summary>
            public bool EnableSsl { get; set; } = true;
        }
    }
}
