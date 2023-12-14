using Service_Billing.Models;

namespace Service_Billing.ViewModels
{
    public class AllBillsViewModel
    {
        public IEnumerable<Bill> Bills { get; }

        public AllBillsViewModel(IEnumerable<Bill> bills) 
        { 
            Bills = bills;
        }
    }
}
