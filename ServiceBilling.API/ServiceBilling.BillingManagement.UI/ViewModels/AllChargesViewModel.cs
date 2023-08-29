using ServiceBilling.BillingManagement.UI.Models;

namespace ServiceBilling.BillingManagement.UI.ViewModels
{
    public class AllChargesViewModel
    {
        public IEnumerable<Charge> Charges { get; }
        public IEnumerable<ServiceCategory> ServiceCategories { get; }
        public IEnumerable<ClientAccount> ClientAccounts { get; }

        public AllChargesViewModel(IEnumerable<Charge> charges, IEnumerable<ServiceCategory> serviceCategories, IEnumerable<ClientAccount> clients)
        {
            Charges = charges;
            ServiceCategories = serviceCategories;
            ClientAccounts = clients;
        }
    }
}
