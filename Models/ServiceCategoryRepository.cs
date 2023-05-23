using Service_Billing.Data;

namespace Service_Billing.Models
{
    public class ServiceCategoryRepository: IServiceCategoryRepository
    {
        private readonly ServiceBillingContext _context;
        public ServiceCategoryRepository(ServiceBillingContext context)
        {
            _context = context;
        }

        public void Add(ServiceCategory serviceCategory)
        {
            throw new NotImplementedException();
        }

        public void Delete(ServiceCategory serviceCategory)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ServiceCategory> GetAll()
        {
            return _context.serviceCategories.OrderBy(s => s.name);
        }

        public ServiceCategory? GetById(int id)
        {
            return _context.serviceCategories.FirstOrDefault(s => s.serviceId == id);
        }

        public IEnumerable<ServiceCategory> Search(string queryString)
        {
            return _context.serviceCategories.Where(s => s.name == queryString);
        }

        public void Update(ServiceCategory serviceCategory)
        {
            throw new NotImplementedException();
        }
    }
}
