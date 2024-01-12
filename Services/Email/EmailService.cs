using MailKit.Net.Smtp;
using MailKit.Security;
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
                email.Sender = MailboxAddress.Parse("andre.lashley@gov.bc.ca");
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;
                var builder = new BodyBuilder();
                builder.HtmlBody = message;
                email.Body = builder.ToMessageBody();
                using var smtp = new SmtpClient();
                smtp.Connect("apps.smtp.gov.bc.ca", 587, true);
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
