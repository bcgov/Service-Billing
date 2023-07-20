using ServiceBilling.API.Application.Contracts.Infrastructure;
using ServiceBilling.API.Application.Models.Mail;

namespace ServiceBilling.API.Infrastructure.Mail
{
    public class EmailService : IEmailService
    {
        public async Task<bool> SendEmail(Email email)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));

            Console.WriteLine($"Sending email to {email.To}");

            return true;
        }
    }
}
