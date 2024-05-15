using Service_Billing.Data;
using Service_Billing.Models;

namespace Service_Billing.Models.Repositories
{
    public class MinistryRepository: IMinistryRepository
    {
        private readonly ServiceBillingContext _billingContext;

        public MinistryRepository(ServiceBillingContext billingContext)
        {
            _billingContext = billingContext;
        }

        public IEnumerable<Ministry> GetAll()
        {
            return _billingContext.Ministries.OrderBy(x => x.Acronym);
        }

        public Ministry? GetById(int id)
        {
            return _billingContext.Ministries.FirstOrDefault(x => x.Id == id);
        }

        public async Task Update(Ministry ministry)
        {
            _billingContext.Update(ministry);
            await _billingContext.SaveChangesAsync();
        }
        public async Task Save(Ministry ministry)
        {
            await _billingContext.AddAsync(ministry);
            await _billingContext.SaveChangesAsync();
        }
    }
}
