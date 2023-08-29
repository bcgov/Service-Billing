namespace ServiceBilling.BillingManagement.UI.Services.Mail
{
    public interface IEmailService
    {
        Task<bool> SendEmail();
    }
}
