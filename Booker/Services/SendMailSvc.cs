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

        private async Task Send(MailMessage message)
        {
            using SmtpClient smtpClient = new SmtpClient();

            smtpClient.Host = _smtpSettings.Server;
            smtpClient.Port = _smtpSettings.Port;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password); 


            try
            {
                await smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                _log.LogError(new EventId(),ex,ex.Message,ex.InnerException?.Message);
            }
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MailMessage();
            message.Subject = subject;
            message.Body = htmlMessage;
            message.To.Add(email);
            message.From = new MailAddress("no-reply@textbooker.pl");
            message.IsBodyHtml = true;
            await this.Send(message);
        }

        public class SmtpSettings
        {
            public string Server { get; set; }
            public int Port { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
