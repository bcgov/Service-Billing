using Microsoft.EntityFrameworkCore;
using Service_Billing.Data;
using Service_Billing.Models;

namespace Service_Billing.Models.Repositories
{
    public class ServiceCategoryRepository: IServiceCategoryRepository
    {
        private readonly ServiceBillingContext _context;
        public ServiceCategoryRepository(ServiceBillingContext context)
        {
            _context = context;
        }

        public int Add(ServiceCategory serviceCategory)
        {
            _context.Add(serviceCategory);
            _context.SaveChanges();
            return serviceCategory.ServiceId;
        }

        public void Delete(ServiceCategory serviceCategory)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ServiceCategory> GetAll()
        {
            return _context.ServiceCategories.OrderBy(s => s.Name).Include(s => s.BusArea);
        }

        public ServiceCategory? GetById(int? id)
        {
            return _context.ServiceCategories.Include(s => s.BusArea).FirstOrDefault(s => s.ServiceId == id);
        }

        public IEnumerable<ServiceCategory> Search(string queryString)
        {
            return _context.ServiceCategories.Where(s => s.Name == queryString);
        }

        public void Update(ServiceCategory serviceCategory)
        {
            _context.Update(serviceCategory);
            _context.SaveChanges(true);
        }
    }
}
