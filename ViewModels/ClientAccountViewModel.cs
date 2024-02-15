using Service_Billing.Models;

namespace Service_Billing.ViewModels
{
    public class ClientAccountViewModel
    {
        public IEnumerable<ClientAccount> ClientAccounts { get; }

        //now that we don't have a separate table for client teams, this model is probably redundant. 
        public ClientAccountViewModel(IEnumerable<ClientAccount> clients)
        {
  
        }

    
    }
}
