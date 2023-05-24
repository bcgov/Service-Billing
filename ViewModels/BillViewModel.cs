using Service_Billing.Models;

namespace Service_Billing.ViewModels
{
    public class BillViewModel
    {
        public IEnumerable<Bill> Bills { get; }
        public IEnumerable<ServiceCategory> ServiceCategories { get; }
        public IEnumerable<ClientAccount> ClientAccounts { get; }

        public BillViewModel(IEnumerable<Bill> bills, IEnumerable<ServiceCategory> serviceCategories, IEnumerable<ClientAccount> clients) 
        { 
            Bills = bills;
            ServiceCategories = serviceCategories;
            ClientAccounts = clients;
        }
    }
}
