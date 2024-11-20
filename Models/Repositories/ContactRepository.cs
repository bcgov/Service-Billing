using Service_Billing.Data;

namespace Service_Billing.Models.Repositories
{
    public class ContactRepository : IContactRepository
    {
        private readonly ServiceBillingContext _billingContext;
        private readonly ILogger<BillRepository> _logger;

        public ContactRepository(ServiceBillingContext billingContext, ILogger<BillRepository> logger)
        {
            _billingContext = billingContext;
            _logger = logger;
        }

        public void Add(Contact contact)
        {
            throw new NotImplementedException();
        }

        public Contact Get(int id)
        {
            return _billingContext.Contacts.FirstOrDefault(x => x.Id == id);
        }

        public Contact GetByEmail(string email)
        {
            throw new NotImplementedException();
        }
    }
}
