namespace ServiceBilling.BillingManagement.UI.Models.Repositories
{
    public interface IMinistryRepository
    {
        IEnumerable<Ministry> GetAll();
    }
}
