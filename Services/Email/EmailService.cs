using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Service_Billing.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;
        private readonly ILogger<EmailService> _logger;
        public EmailService(IOptions<MailSettings> mailSettings, ILogger<EmailService> logger)
        {
            _mailSettings = mailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmail(string to, string subject, string message)
        {
            try
            {
                var email = new MimeMessage();
                var addr = new MailboxAddress("Lashley, Andre CITZ:EX", "Andre.Lashley@gov.bc.ca");
                email.From.Add(new MailboxAddress("Lashley, Andre CITZ:EX", "Andre.Lashley@gov.bc.ca"));
                email.To.Add(new MailboxAddress("Lashley, Andre CITZ:EX", "Andre.Lashley@gov.bc.ca"));
                //email.Sender = addr;
                email.To.Add(addr);
                email.Subject = subject;
                var builder = new BodyBuilder();
                builder.HtmlBody = message;
                email.Body = builder.ToMessageBody();
                using var smtp = new SmtpClient();
                smtp.Connect("apps.smtp.gov.bc.ca", 25, false);
                await smtp.SendAsync(email);
                smtp.Disconnect(true);

                _logger.LogInformation("Email sent successfully.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error sending email");
                return false;
            }

            return true;
        }
    }
}
