using Service_Billing.Models;

namespace Service_Billing.ViewModels
{
    public class EditChargeViewModel
    {
        public Bill Bill { get; set; }
        public FiscalHistory? FiscalHistory { get; set; }

        public IEnumerable<ServiceCategory>? Categories { get; set; }

        public EditChargeViewModel (Bill bill, FiscalHistory fiscalHistory )
        {
            Bill = bill;
            FiscalHistory = fiscalHistory;
        }

        public EditChargeViewModel()
        {
            Bill = new Bill();
            FiscalHistory = new FiscalHistory();
        }
    }
}
