using Service_Billing.Data;

namespace Service_Billing.Models
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
    }
}
