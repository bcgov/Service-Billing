namespace Service_Billing.Models
{
    public interface IMinistryRepository
    {
        IEnumerable<Ministry> GetAll();
    }
}
