using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Service_Billing.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;
        public EmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task<bool> SendEmail(string to, string subject, string message)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse("gdxbilling@gov.bc.ca");
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            var builder = new BodyBuilder();
            builder.HtmlBody = message;
            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            smtp.Connect(_mailSettings.Host, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(to, _mailSettings.Password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);

            return true;
        }
    }
}
