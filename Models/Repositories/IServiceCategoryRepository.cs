namespace Service_Billing.Models.Repositories
{
    public interface IServiceCategoryRepository
    {
        IEnumerable<ServiceCategory> GetAll();
        int Add(ServiceCategory serviceCategory);
        Task Update(ServiceCategory serviceCategory, string userName);
        void Delete(ServiceCategory serviceCategory);
        IEnumerable<ServiceCategory> Search(string queryString);
        ServiceCategory? GetById(int? id);
    }
}
