namespace ServiceBilling.BillingManagement.UI.Models.Repositories
{
    public interface IServiceCategoryRepository
    {
        IEnumerable<ServiceCategory> GetAllServiceCategories();
        Task<IEnumerable<ServiceCategory>> GetAllServiceCategoriesAsync();
        Task<int> AddServiceCategoryAsync(ServiceCategory serviceCategory);
    }
}
