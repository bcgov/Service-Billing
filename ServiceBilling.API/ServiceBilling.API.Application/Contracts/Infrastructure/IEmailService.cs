using ServiceBilling.API.Application.Models.Mail;

namespace ServiceBilling.API.Application.Contracts.Infrastructure
{
    public interface IEmailService
    {
        Task<bool> SendEmail(Email email);
    }
}
