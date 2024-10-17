using Service_Billing.Models;

namespace Service_Billing.ViewModels
{
    public class EditChargeViewModel
    {
        public Bill Bill { get; set; }
        public FiscalHistory? FiscalHistory { get; set; }

        public EditChargeViewModel (Bill bill, FiscalHistory? fiscalHistory = null)
        {
            Bill = bill;
            FiscalHistory = fiscalHistory;
        }
    }
}
