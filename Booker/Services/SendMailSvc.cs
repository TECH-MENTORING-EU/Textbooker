using System.Net.Mail;
using System.Net;
using NuGet.Packaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static Booker.Services.SendMailSvc;

namespace Booker.Services
{
    public class SendMailSvc
    {
        SmtpSettings _smtpSettings;
        ILogger<SendMailSvc> _log;

      
        public SendMailSvc(ILogger<SendMailSvc> logger, IOptions<SmtpSettings> smtpSettings)
        {
            _log = logger;
            _smtpSettings = smtpSettings.Value;
        }

        /*
        parameter example:

          MailMessage message = new MailMessage();
          MailAddress fromAddress = new MailAddress("random@textbooker.pl");
          message.From = fromAddress;
          message.Subject = "tytuł wiadomości";
          message.IsBodyHtml = true;
          message.Body = "<h1>treść wiadomości</h1>";
          message.To.Add("wgw31336@msssg.com");     
        
       */
        public async Task Send(MailMessage message)
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

        public class SmtpSettings
        {
            public string Server { get; set; }
            public int Port { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
