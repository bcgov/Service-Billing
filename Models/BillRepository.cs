using Microsoft.EntityFrameworkCore;
using Service_Billing.Data;

namespace Service_Billing.Models
{
    public class BillRepository : IBillRepositroy
    {
        private readonly ServiceBillingContext _billingContext;
        public BillRepository  (ServiceBillingContext billingContext)
        {
            _billingContext = billingContext;
        }

        public IEnumerable<Bill> AllBills => _billingContext.bills.OrderBy(b => b.title);

        public Bill? GetBill(int id)
        {
            return _billingContext.bills.FirstOrDefault(b => b.id == id);
        }

        public IEnumerable<Bill> SearchBillsByTitle(string searchQuery)
        {
            return _billingContext.bills.Where(b => b.title.Contains(searchQuery));
        }

        IEnumerable<Bill> IBillRepositroy.GetBillsByAuthority(string expenseAuthority)
        {
            throw new NotImplementedException();
        }

        IEnumerable<Bill> IBillRepositroy.GetBillsByBillingCycle(DateOnly billingCycle)
        {
            throw new NotImplementedException();
        }

        IEnumerable<Bill> IBillRepositroy.GetBillsByClientId(int clientId)
        {
            throw new NotImplementedException();
        }

        IEnumerable<Bill> IBillRepositroy.GetBillsByDateRange(DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        IEnumerable<Bill> IBillRepositroy.GetBillsByServiceCategory(int serviceCategoryId)
        {
            throw new NotImplementedException();
        }
    }
}
