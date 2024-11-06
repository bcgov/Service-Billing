using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Service_Billing.Data;
using System;
using System.Reflection;

namespace Service_Billing.Models.Repositories
{
    public class BillRepository : IBillRepository
    {
        private readonly ServiceBillingContext _billingContext;
        private readonly IFiscalPeriodRepository _fiscalPeriodRepository;
        private readonly IFiscalHistoryRepository _fiscalHistoryRepository;
        private readonly IChangeLogRepository _changeLogRepository;
        private readonly ILogger<BillRepository> _logger;
        public BillRepository(ServiceBillingContext billingContext,
            IFiscalPeriodRepository fiscalPeriodRepository,
            ILogger<BillRepository> logger,
            IFiscalHistoryRepository fiscalHistoryRepository,
            IChangeLogRepository changeLogRepository)
        {
            _billingContext = billingContext;
            _fiscalPeriodRepository = fiscalPeriodRepository;
            _logger = logger;
            _fiscalHistoryRepository = fiscalHistoryRepository;
            _changeLogRepository = changeLogRepository;
        }

        public IEnumerable<Bill> AllBills => _billingContext.Bills.AsNoTracking()
                .Include(c => c.ServiceCategory)
                .Include(c => c.MostRecentActiveFiscalPeriod)
                .Include(c => c.PreviousFiscalRecords)
                .Include(bill => bill.ClientAccount);

        public string DetermineCurrentQuarter(DateTime? date = null)
        {
            DateTime today = DateTime.Today;
            if (date != null)
                today = date.Value;
            string quarter = "";
            int year1 = today.Year;
            int year2 = (today.Year + 1);

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
                    return $"Fiscal {(year1 - 1).ToString().Substring(2)}/{year1.ToString().Substring(2)} {quarter}";
            }

            return $"Fiscal {year1.ToString().Substring(2)}/{year2.ToString().Substring(2)} {quarter}";
        }
        public DateTime DetermineStartOfCurrentQuarter()
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
        public DateTime DetermineStartOfNextQuarter()
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

        public DateTime DetermineEndOfQuarter(DateTime quarterStart)
        {
            switch (quarterStart.Month)
            {
                case 4:
                case 5:
                case 6:
                    return new DateTime(quarterStart.Year, 6, 30);
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
            try
            {
                _logger.LogInformation("Promoting charges to new quarter...");
                // determine limits of current fiscal quarter
                DateTimeOffset quarterStart = DetermineStartOfCurrentQuarter();
                _logger.LogInformation($"quarter start date: {quarterStart.Date}");
                DateTimeOffset quarterEnd = DetermineEndOfQuarter(quarterStart.Date);
                _logger.LogInformation($"quarter end date: {quarterEnd.Date}");
                // list which services are fixed consumptions. Ignore charges where UOM is not month
                List<int> fixedServiceIds = GetFixedServices();
                List<int> oneTimeServiceIds = GetOneTimeServices();
                string newQuarter = DetermineCurrentQuarter();
                _logger.LogInformation($"new quarter string is \"{newQuarter}\"");
                // see if we have an entry in the DB for this fiscal period (we shouldn't), and create it if it doesn't exist
                FiscalPeriod newFiscalPeriod = _fiscalPeriodRepository.GetByFiscalQuarterString(newQuarter);
                if (newFiscalPeriod == null)
                {
                    newFiscalPeriod = new FiscalPeriod(newQuarter);
                    _fiscalPeriodRepository.SaveFiscalPeriod(newFiscalPeriod);
                }
                else
                    _logger.LogWarning($"Promoting charges to new quarter, but there already seems to be an entry for {newQuarter}. That's weird...");
                if (newFiscalPeriod == null)
                {
                    throw new Exception("An error occurred while creating a new Fiscal Period database entry.");
                }
                IEnumerable<Bill> billsToPromote = _billingContext.Bills.Where(b => b.ServiceCategoryId != null
                && fixedServiceIds.Contains((int)b.ServiceCategoryId)
                && (b.EndDate == null || b.EndDate > quarterStart)
                && b.IsActive);

                foreach (Bill bill in billsToPromote)
                    await PromoteCharge(bill, newFiscalPeriod, quarterStart, false);

                await _billingContext.SaveChangesAsync();
                _logger.LogInformation("Charges promoted to new quarter!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"error while trying to promote charges to new quarter!");
                _logger.LogError(ex.ToString());
            }
        }

        public async Task PromoteCharge(Bill bill, FiscalPeriod newFiscalPeriod, DateTimeOffset? quarterStart = null, bool saveDBChanges = true)
        {
            try
            {
                if (quarterStart == null)
                    quarterStart = DetermineStartOfCurrentQuarter();
                List<int> recordedPeriodIds = _fiscalHistoryRepository.GetFiscalHistoriesByChargeId(bill.Id).Select(b => b.PeriodId).ToList();
                if (bill.CurrentFiscalPeriodId == newFiscalPeriod.Id) // make sure charge has no fiscal history for the new quarter
                {
                    _logger.LogWarning($"tried promoting bill with ID: {bill.Id} to a new FiscalPeriod, but it's CurrentFiscalPeriodId matches the new FiscalPeriod.Id ({newFiscalPeriod.Id}). Skipping this Charge.");
                    return; //don't add anything more than once.
                }
                //handle fiscal period tracking.
                ServiceCategory? category = bill.ServiceCategory;
                decimal unitPriceAtFiscal = 0;
                if (!decimal.TryParse(category?.Costs, out unitPriceAtFiscal))
                    _logger.LogWarning($"Could not find a unit price for the Service Category belonging to charge with Id {bill.Id}. ServiceCategory Id: {bill.ServiceCategoryId}");

                AddBillFiscalHistoryToContext(bill.Id, bill.CurrentFiscalPeriodId, newFiscalPeriod.Id, unitPriceAtFiscal, bill.Quantity.Value, bill.Notes);

                bill.CurrentFiscalPeriodId = newFiscalPeriod.Id;
                decimal newQuantityForCharge = GetBillQuantityForNewQuarter(bill, quarterStart.Value.Date);
                if (bill.Quantity != newQuantityForCharge)
                {
                    bill.Quantity = newQuantityForCharge;
                    if (bill.ServiceCategory != null && !String.IsNullOrEmpty(bill.ServiceCategory.Costs))
                    {
                        decimal unitPrice;
                        if (!decimal.TryParse(bill.ServiceCategory.Costs, out unitPrice))
                        {
                            _logger.LogError($"No unit Price found for bill with ID: {bill.Id}. The service category {bill.ServiceCategory.Name} has no unit price set.");
                        }
                        else
                            bill.Amount = decimal.Parse(bill.ServiceCategory.Costs) * newQuantityForCharge;
                    }
                    else
                        _logger.LogError($"No service category found for charge with ID: {bill.Id}! Could not update charge amount!");

                }
                _billingContext.Update(bill);

                if (saveDBChanges)
                    await _billingContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error trying to promote bill: {bill.Id} to new quarter with PeriodId {newFiscalPeriod.Id}.");
                _logger.LogError(ex.Message);
            }
        }

        private void AddBillFiscalHistoryToContext(int chargeId, int currentFiscalId, int newFiscalId, decimal unitPriceAtFiscal, decimal quantity, string notes)
        {
            FiscalHistory fiscalHistory = new FiscalHistory(chargeId, currentFiscalId, unitPriceAtFiscal, quantity, notes);
            if (_fiscalHistoryRepository.GetFiscalHistoryByIdAndChargeId(newFiscalId, chargeId) == null) // and it should be null!
            {
                _billingContext.FiscalHistory.Add(fiscalHistory);
            }
        }

        private decimal GetBillQuantityForNewQuarter(Bill bill, DateTime quarterStart)
        {
            if (bill.EndDate == null)
                return (decimal)3.0;

            else //find out how many months they should be billed for
            {
                int billEndMonth = bill.EndDate.Value.Month;
                int quarterStartMonth = quarterStart.Month;
                if (bill.EndDate.Value.Year > quarterStart.Year)
                {
                    int monthsScalar = bill.EndDate.Value.Year - quarterStart.Year;
                    billEndMonth *= (monthsScalar * 12);
                }
                int duration = billEndMonth - quarterStartMonth + 1;

                return duration < 3 ? (decimal)duration : (decimal)3.0;
            }
        }

        public IEnumerable<Bill> GetCurrentQuarterBills()
        {
            string fiscalPeriod = DetermineCurrentQuarter();
            return _billingContext.Bills.AsNoTracking()
                .Include(c => c.ServiceCategory)
                .Include(bill => bill.ClientAccount)
                .Where(b => b.MostRecentActiveFiscalPeriod.Period == fiscalPeriod);
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
            catch (Exception ex)
            {

            }
            return string.Empty;
        }

        public IEnumerable<FiscalHistory> GetPreviousQuarterChargeHistory()
        {
            try
            {
                string previousQuarterString = GetPreviousQuarterString();
                FiscalPeriod? previousQuarter = _fiscalPeriodRepository.GetByFiscalQuarterString(previousQuarterString);
                if (previousQuarter == null)
                {
                    throw new Exception($"No fiscal period entry was found for {previousQuarterString}");
                }
                return _fiscalHistoryRepository.GetFiscalHistoryByFiscalPeriodId(previousQuarter.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        public Bill? GetBill(int id)
        {
            return _billingContext.Bills
                .Include(c => c.ServiceCategory)
                .Include(bill => bill.ClientAccount)
                .Include(c => c.PreviousFiscalRecords!).ThenInclude(h => h.FiscalPeriod)
                .Include(bill => bill.MostRecentActiveFiscalPeriod)
                .First(b => b.Id == id);
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

        public async Task<int> CreateBill(Bill bill)
        {
            Bill newBill = new Bill();
            newBill.Title = bill.Title;
            newBill.ServiceCategoryId = bill.ServiceCategoryId;
            newBill.Amount = bill.Amount;
            newBill.BillingCycle = bill.BillingCycle;
            newBill.ClientAccountId = bill.ClientAccountId;
            //    newBill.MostRecentActiveFiscalPeriod = bill.MostRecentActiveFiscalPeriod;
            newBill.CurrentFiscalPeriodId = bill.CurrentFiscalPeriodId;
            newBill.IdirOrUrl = bill.IdirOrUrl;
            newBill.StartDate = bill.StartDate;
            newBill.IsActive = bill.IsActive;
            newBill.TicketNumberAndRequester = bill.TicketNumberAndRequester;
            newBill.Quantity = bill.Quantity;
            newBill.Notes = bill.Notes;
            newBill.EndDate = bill.EndDate;
            newBill.DateModified = bill.DateModified;
            newBill.CreatedBy = bill.CreatedBy;

            await _billingContext.AddAsync(newBill);
            await _billingContext.SaveChangesAsync();
            return newBill.Id;
        }

        public async Task Update(Bill editedBill, string userName = "system")
        {
            Bill? bill = GetBill(editedBill.Id);
            if (bill == null)
            {
                throw new Exception("Could not retrieve bill from database");
            }
            _billingContext.ChangeTracker.DetectChanges();
            if (editedBill != null)
            {
                var properties = typeof(Bill).GetProperties();

                foreach (var property in properties)
                {
                    if (property.Name == "DateModified")
                        continue;
                    // Check if the property can be written to and is not a navigation property
                    if (property.PropertyType != typeof(ServiceCategory) 
                    && property.PropertyType != typeof(ClientAccount)
                    && property.PropertyType != typeof(FiscalPeriod)
                    && property.Name != "DateCreated"
                    && property.PropertyType != typeof(ICollection<FiscalHistory>))
                    {
                        var originalValue = property.GetValue(bill);
                        var modifiedValue = property.GetValue(editedBill);

                        // Check for differences
                        if (!Equals(originalValue, modifiedValue))
                        {
                            if(property.PropertyType == typeof(DateTimeOffset))
                            {
                                DateTimeOffset originalDate;
                                DateTimeOffset modifiedDate;
                                if(DateTimeOffset.TryParse(property.GetValue(bill).ToString(), out originalDate) 
                                && DateTimeOffset.TryParse(property.GetValue(bill).ToString(), out modifiedDate))
                                {
                                    if (Equals(originalDate.Date.ToShortDateString(), modifiedDate.Date.ToShortDateString())) // don't care if it's just a matter of hours
                                        continue;
                                }
                                else
                                {
                                    throw new Exception("Could not parse dates from model");
                                }
                            }
                            property.SetValue(bill, modifiedValue);
                            _billingContext.Entry(bill).Property(property.Name).IsModified = true;
                        }
                    }
                }

                await _changeLogRepository.MakeChangeLogEntry(bill, userName);
                _billingContext.Update(bill);
                await _billingContext.SaveChangesAsync();
            }
        }

        public async Task UpdateAllChargesForServiceCategory(int serviceCategoryId)
        {
            IEnumerable<Bill> charges = _billingContext.Bills.Where(b => b.ServiceCategoryId == serviceCategoryId);
            ServiceCategory? service = _billingContext.ServiceCategories.FirstOrDefault(s => s.ServiceId == serviceCategoryId);
            if (service != null)
            {
                decimal newCost;
                if (decimal.TryParse(service.Costs, out newCost))
                {
                    foreach (Bill charge in charges)
                    {
                        charge.Amount = newCost * charge.Quantity;
                        await Update(charge);
                    }
                }
            }
        }

        public Task Update(Bill bill)
        {
            return Update(bill, "Billing System"); // for the test suite.
        }
    }
}