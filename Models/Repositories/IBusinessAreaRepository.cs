namespace Service_Billing.Models.Repositories
{
    public interface IBusinessAreaRepository
    {
        IEnumerable<BusinessArea> GetAll();
        BusinessArea? GetById(int id);

        int Add(BusinessArea businessArea);
    }
}
