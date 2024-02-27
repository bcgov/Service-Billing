
using Service_Billing.Data;

namespace Service_Billing.Models.Repositories
{
    public class BusinessAreaRepository : IBusinessAreaRepository
    {
        private readonly ServiceBillingContext _context;
        public BusinessAreaRepository(ServiceBillingContext context)
        {
            _context = context;
        }
        public IEnumerable<BusinessArea> GetAll()
        {
            return _context.BusAreas;
        }

        public BusinessArea? GetById(int id)
        {
            return _context.BusAreas.FirstOrDefault(x => x.Id == id);
        }
    }
}
