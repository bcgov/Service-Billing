using Microsoft.Extensions.Options;

namespace ServiceBilling.BillingManagement.UI.Services.Mail
{
    public class EmailService : IEmailService
    {
        public EmailSettings _emailSettings { get; }

        public EmailService(IOptions<EmailSettings> mailSettings)
        {
            _emailSettings = mailSettings.Value;
        }

        public async Task<bool> SendEmail() // Email email
        {
            await Task.Delay(TimeSpan.FromSeconds(1));

            Console.WriteLine($"Sending email to {"email.To"}");

            return true;
        }
    }
}
