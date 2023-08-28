using ServiceBilling.BillingManagement.UI.Models;

namespace ServiceBilling.BillingManagement.UI.ViewModels
{
    public class ClientAccountViewModel
    {
        public IEnumerable<ClientAccount> ClientAccounts { get; }

        public ClientAccountViewModel(IEnumerable<ClientAccount> clients)
        {
            ClientAccounts = clients;
        }
    }
}
