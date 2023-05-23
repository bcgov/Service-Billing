namespace Service_Billing.Models
{
    public interface IServiceCategoryRepository
    {
        IEnumerable<ServiceCategory> GetAll();
        void Add(ServiceCategory serviceCategory);
        void Update(ServiceCategory serviceCategory);
        void Delete(ServiceCategory serviceCategory);
        IEnumerable<ServiceCategory> Search(string queryString);
        ServiceCategory? GetById(int id);
    }
}
