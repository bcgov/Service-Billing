using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Service_Billing.Data;
using Service_Billing.Models;
using System.Linq;

namespace Service_Billing.Models.Repositories
{
    public class BillRepository : IBillRepository
    {
        private readonly ServiceBillingContext _billingContext;
        private readonly IFiscalPeriodRepository _fiscalPeriodRepository;
        public BillRepository  (ServiceBillingContext billingContext, IFiscalPeriodRepository fiscalPeriodRepository)
        {
            _billingContext = billingContext;
            _fiscalPeriodRepository = fiscalPeriodRepository;  
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
        private DateTime DetermineStartOfCurrentQuarter()
        {
            DateTime quarter = DateTime.Today;
            switch (quarter.Month)
            { // April 1 to March 31
                case 4:
                case 5:
                case 6:
                    return new DateTime(quarter.Year, 4, 1);
                case 7:
                case 8:
                case 9:
                    return new DateTime(quarter.Year, 7, 1);
                case 10:
                case 11:
                case 12:
                    return new DateTime(quarter.Year, 10, 1);
                case 1:
                case 2:
                case 3:
                    return new DateTime(quarter.Year, 1, 1);

                default:
                    return DateTime.Today;
            }
        }
        private DateTime DetermineStartOfNextQuarter()
        {
            DateTime quarter = DateTime.Today;
            switch (quarter.Month)
            { // April 1 to March 31
                case 4:
                case 5:
                case 6:
                    return new DateTime(quarter.Year, 7, 1);
                case 7:
                case 8:
                case 9:
                    return new DateTime(quarter.Year, 10, 1);
                case 10:
                case 11:
                case 12:
                    return new DateTime(quarter.Year, 1, 1);
                case 1:
                case 2:
                case 3:
                    return new DateTime(quarter.Year, 4, 1);

                default:
                    return DateTime.Today;
            }
        }

        private DateTime DetermineEndOfQuarter(DateTime quarterStart)
        {
            switch (quarterStart.Month)
            {
                case 4:
                case 5:
                case 6:
                    return new DateTime(quarterStart.Year, 6, 31);
                case 7:
                case 8:
                case 9:
                    return new DateTime(quarterStart.Year, 9, 30);
                case 10:
                case 11:
                case 12:
                    return new DateTime(quarterStart.Year, 12, 31);
                case 1:
                case 2:
                case 3:
                    return new DateTime(quarterStart.Year, 3, 31);

                default:
                    return DateTime.Today;
            }
        }

        public List<int> GetFixedServices()
        {
            IEnumerable<ServiceCategory> serviceCategories = _billingContext.ServiceCategories;
            List<int> fixedServiceIds = serviceCategories.Where(x => !String.IsNullOrEmpty(x.UOM)
            && x.UOM.ToLower() == "month"
            && x.IsActive)
                .Select(x => x.ServiceId).ToList();

            return fixedServiceIds;
        }
        public List<int> GetOneTimeServices()
        {
            IEnumerable<ServiceCategory> serviceCategories = _billingContext.ServiceCategories;
            List<int> oneTimeServiceIds = serviceCategories.Where(x => x.UOM == null || x.UOM.ToLower() != "month"
            && x.IsActive)
               .Select(x => x.ServiceId).ToList();

            return oneTimeServiceIds;
        }
        public async Task PromoteChargesToNewQuarter()
        {
            // determine limits of current fiscal quarter
            DateTime quarterStart = DetermineStartOfCurrentQuarter();
            DateTime quarterEnd = DetermineEndOfQuarter(quarterStart);
            // list which services are fixed consumptions. Ignore charges where UOM is not month
            List<int> fixedServiceIds = GetFixedServices();
            List<int> oneTimeServiceIds = GetOneTimeServices();
            string newQuarter = DetermineCurrentQuarter();

            IEnumerable<Bill> billsToPromote = _billingContext.Bills.Where(b => b.ServiceCategoryId != null
            && fixedServiceIds.Contains((int)b.ServiceCategoryId)
            && (b.EndDate == null || b.EndDate > quarterStart)
            && b.IsActive);
            
            foreach (Bill bill in billsToPromote)
            {
                _fiscalPeriodRepository.UpdateRecord(bill.Id, bill.FiscalPeriod, bill.Amount);
                bill.FiscalPeriod = newQuarter;
                _billingContext.Update(bill);
            }

            await _billingContext.SaveChangesAsync();
        }

        public IEnumerable<Bill> GetCurrentQuarterBills()
        {
            string fiscalPeriod = DetermineCurrentQuarter();
            return _billingContext.Bills.Where(b => b.FiscalPeriod == fiscalPeriod);
        }

        public string GetPreviousQuarterString()
        {
            try
            {
                string currentFiscalPeriod = DetermineCurrentQuarter();
                //we need to go to quarter 4 of last year
                if (currentFiscalPeriod.Contains("Quarter 1"))
                { //Fiscal 23/24 Quarter 1
                    int year;
                    string yearString = currentFiscalPeriod.Substring(10, 2);
                    if (!int.TryParse(yearString, null, out year))
                    {
                        throw new Exception("Could not parse year from Fiscal Period string");
                    } //quarter 4 is Jan. 1 - Mar. 31

                    return $"Fiscal {year - 2}/{year - 1} Quarter 4";
                }
                else
                {
                    int quarter;
                    if (!int.TryParse(currentFiscalPeriod.Last().ToString(), null, out quarter))
                    {
                        throw new Exception("Could not parse Quarter from Fiscal Period string");
                    }
                    currentFiscalPeriod = currentFiscalPeriod.Remove(currentFiscalPeriod.Length - 1);
                    return currentFiscalPeriod += (quarter - 1);

                }
            
            }
            catch(Exception ex)
            {
                
            }
            return string.Empty;
        }

        public IEnumerable<Bill> GetPreviousQuarterBills()
        {
            try
            {
                Dictionary<int, decimal?> previousQuarterChargeIds = _fiscalPeriodRepository.ChargeIdsAndCostByFiscalPeriod(GetPreviousQuarterString());
                IEnumerable<Bill> previousQuarterBills = _billingContext.Bills.Where(b => previousQuarterChargeIds.Keys.Contains(b.Id));
                
            }
            catch(Exception e) 
            {
                //Todo: Add error logging.
            }

            return null;
        }

        public IEnumerable<Bill> GetNextQuarterBills()
        {
            try
            {
                DateTime nextQuarterStart = DetermineStartOfNextQuarter();
                string currentFiscalPeriod = DetermineCurrentQuarter();
                List<int> fixedServiceIds = GetFixedServices();
                return _billingContext.Bills.Where(b => b.ServiceCategoryId != null
                   && fixedServiceIds.Contains((int)b.ServiceCategoryId)
                   && (b.EndDate == null || b.EndDate > nextQuarterStart));
            }
            catch (Exception e)
            {
                // Todo: this interface could really use some good old logging. 
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

        IEnumerable<Bill> IBillRepository.GetBillsByAuthority(string expenseAuthority)
        {
            throw new NotImplementedException();
        }

        IEnumerable<Bill> IBillRepository.GetBillsByBillingCycle(DateOnly billingCycle)
        {
            throw new NotImplementedException();
        }

        IEnumerable<Bill> IBillRepository.GetBillsByClientId(int clientId)
        {
            return _billingContext.Bills.Where(b => b.ClientAccountId == clientId);
        }

        IEnumerable<Bill> IBillRepository.GetBillsByDateRange(DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        IEnumerable<Bill> IBillRepository.GetBillsByServiceCategory(int serviceCategoryId)
        {
            throw new NotImplementedException();
        }

        public async Task CreateBill(Bill bill)
        {
            await _billingContext.AddAsync(bill);
            await _billingContext.SaveChangesAsync();
        }

        public async Task Update(Bill bill)
        {
            _billingContext.Update(bill);
            await _billingContext.SaveChangesAsync();
        }

    }
}
