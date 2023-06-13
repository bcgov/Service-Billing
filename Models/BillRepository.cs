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

        public string DetermineCurrentQuarter()
        {
            DateTime today = DateTime.Today;
            string quarter = "";
            string year1 = today.Year.ToString();
            string year2 = (today.Year + 1).ToString();

            switch (today.Month)
            {
                case 4:
                case 5:
                case 6:
                    quarter = "Quarter 1";
                    break;
                case 7:
                case 8:
                case 9:
                    quarter = "Quarter 2";
                    break;
                case 10:
                case 11:
                case 12:
                    quarter = "Quarter 3";
                    break;
                case 1:
                case 2:
                case 3:
                    quarter = "Quarter 4";
                    break;
            }

            return $"Fiscal {year1.Substring(2)}/{year2.Substring(2)} {quarter}";
        }
        public IEnumerable<Bill> GetCurrentQuarterBills()
        {
            string fiscalPeriod = DetermineCurrentQuarter();
            return _billingContext.bills.Where(b => b.fiscalPeriod == fiscalPeriod);
        }

        public IEnumerable<Bill> GetPreviousQuarterBills()
        {
            throw new NotImplementedException();
        }

        public Bill? GetBill(int id)
        {
            return _billingContext.bills.FirstOrDefault(b => b.id == id);
        }

        public IEnumerable<Bill> SearchBillsByTitle(string searchQuery)
        {
            return _billingContext.bills.Where(b => !string.IsNullOrEmpty(b.title) && b.title.Contains(searchQuery));
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
