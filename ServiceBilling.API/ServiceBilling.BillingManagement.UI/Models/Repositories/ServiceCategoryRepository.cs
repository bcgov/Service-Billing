using Microsoft.EntityFrameworkCore;

namespace ServiceBilling.BillingManagement.UI.Models.Repositories
{
    public class ServiceCategoryRepository : IServiceCategoryRepository
    {
        private readonly DataContext _dataContext;

        public ServiceCategoryRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<int> AddServiceCategoryAsync(ServiceCategory serviceCategory)
        {
            bool serviceCategoryWithSameNameAlreadyExists = 
                _dataContext.ServiceCategories.Any(c => c.Name == serviceCategory.Name);

            if (serviceCategoryWithSameNameAlreadyExists) throw new Exception("A service category with the same name already exists.");


            _dataContext.ServiceCategories.Add(serviceCategory);
            return await _dataContext.SaveChangesAsync();
        }

        public IEnumerable<ServiceCategory> GetAllServiceCategories()
        {
            return _dataContext.ServiceCategories.OrderBy(c => c.Name);
        }

        public async Task<IEnumerable<ServiceCategory>> GetAllServiceCategoriesAsync()
        {
            return await _dataContext.ServiceCategories.OrderBy(c => c.Name).ToListAsync();
        }

        public ServiceCategory? GetById(Guid ServiceCategoryId)
        {
            return _dataContext.ServiceCategories.FirstOrDefault(c => c.ServiceCategoryId == ServiceCategoryId);
        }

        public void Update(ServiceCategory serviceCategory)
        {
            _dataContext.ServiceCategories.Update(serviceCategory);
        }
    }
}
