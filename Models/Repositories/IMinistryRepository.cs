namespace Service_Billing.Models.Repositories
{
    public interface IMinistryRepository
    {
        IEnumerable<Ministry> GetAll();
        Ministry? GetById(int Id);
    }
}
