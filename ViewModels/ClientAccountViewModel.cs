using Service_Billing.Models;

namespace Service_Billing.ViewModels
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
