using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Service_Billing.Data;
using Service_Billing.Models;

namespace Service_Billing.Models.Repositories
{
    public class ServiceCategoryRepository: IServiceCategoryRepository
    {
        private readonly ServiceBillingContext _context;
        private readonly IChangeLogRepository _changeLogRepository;
        public ServiceCategoryRepository(ServiceBillingContext context
        , IChangeLogRepository changeLogRepository)
        {
            _context = context;
            _changeLogRepository = changeLogRepository;
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

        public async Task Update(ServiceCategory serviceCategory, string userName)
        {
            EntityEntry? entry =  await _changeLogRepository.MakeChangeLogAndReturnEntry(serviceCategory, userName);
            ServiceCategory? service = entry?.Entity as ServiceCategory;
            if (service != null)
            {
                _context.Update(service);
                _context.SaveChanges(true);
            }
            else
                throw new Exception($"Something went wrong while trying to update ServiceCategory with ServiceId {serviceCategory.ServiceId}");
        }
    }
}
