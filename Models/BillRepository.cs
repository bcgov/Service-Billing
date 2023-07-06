using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

        public IEnumerable<Bill> AllBills => _billingContext.Bills.OrderBy(b => b.Title);

        public string DetermineCurrentQuarter(DateTime? date = null)
        {
            DateTime today = DateTime.Today;
            if(date != null )
                today = date.Value;
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
            return _billingContext.Bills.Where(b => b.FiscalPeriod == fiscalPeriod);
        }

        public IEnumerable<Bill> GetPreviousQuarterBills()
        {
            try
            {
                string currentFiscalPeriod = DetermineCurrentQuarter();
                //we need to go to quarter 4 of last year
                if (currentFiscalPeriod.Contains("Quarter 1"))
                { //Fiscal 23/24 Quarter 1
                    int year;
                    var x = currentFiscalPeriod.Substring(10, 2);
                    if (!int.TryParse(currentFiscalPeriod.Substring(10, 2), null, out year))
                    {
                        throw new Exception("Could not parse year from Fiscal Period string");
                    }
                    return _billingContext.Bills.Where(b => b.FiscalPeriod == $"Fiscal {year - 2}/{year -1} Quarter 4");
                }
                else
                {
                    int quarter;
                    if (!int.TryParse(currentFiscalPeriod.Last().ToString(), null, out quarter))
                    {
                        throw new Exception("Could not parse Quarter from Fiscal Period string");
                    }
                    currentFiscalPeriod = currentFiscalPeriod.Remove(currentFiscalPeriod.Length - 1);
                    currentFiscalPeriod += (quarter - 1);
                    return _billingContext.Bills.Where(b => b.FiscalPeriod == currentFiscalPeriod);
                }
            }
            catch(Exception e) 
            {
                //Todo: Add error logging.
            }

            return null;
        }

        public Bill? GetBill(int id)
        {
            return _billingContext.Bills.FirstOrDefault(b => b.Id == id);
        }

        public IEnumerable<Bill> SearchBillsByTitle(string searchQuery)
        {
            return _billingContext.Bills.Where(b => !string.IsNullOrEmpty(b.Title) && b.Title.Contains(searchQuery));
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
        public async Task SaveChangesAsync()
        {
            await _billingContext.SaveChangesAsync();
        }

        public async Task CreateBill(Bill bill)
        {
            await _billingContext.AddAsync(bill);
            await _billingContext.SaveChangesAsync();
        }

    }
}
