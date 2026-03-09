using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
                && (b.EndDate == null || b.EndDate >= quarterStart)
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
            // Check if FiscalHistory already exists for the OLD period (currentFiscalId) to prevent duplicates
            if (_fiscalHistoryRepository.GetFiscalHistoryByIdAndChargeId(currentFiscalId, chargeId) == null)
            {
                _billingContext.FiscalHistory.Add(fiscalHistory);
            }
            else
            {
                _logger.LogInformation($"FiscalHistory already exists for charge {chargeId} in period {currentFiscalId}. Skipping duplicate creation.");
            }
        }

        private decimal GetBillQuantityForNewQuarter(Bill bill, DateTime quarterStart)
        {
            DateTime quarterEnd = DetermineEndOfQuarter(quarterStart);
            return CalculateQuantityForQuarter(bill, quarterStart, quarterEnd);
        }

        public decimal CalculateQuantityForQuarter(Bill bill, DateTime quarterStart, DateTime quarterEnd)
        {
            // If bill hasn't started yet in this quarter, quantity is 0
            if (bill.StartDate.HasValue && bill.StartDate.Value.Date > quarterEnd)
                return 0;

            // If bill has already ended before this quarter, quantity is 0
            if (bill.EndDate.HasValue && bill.EndDate.Value.Date < quarterStart)
                return 0;

            // Determine the effective start date within this quarter
            DateTime effectiveStart = bill.StartDate.HasValue && bill.StartDate.Value.Date > quarterStart
                ? bill.StartDate.Value.Date
                : quarterStart;

            // Determine the effective end date within this quarter
            DateTime effectiveEnd = quarterEnd;
            if (bill.EndDate.HasValue && bill.EndDate.Value.Date < quarterEnd)
                effectiveEnd = bill.EndDate.Value.Date;

            // Calculate months between effective start and end
            int months = ((effectiveEnd.Year - effectiveStart.Year) * 12) + effectiveEnd.Month - effectiveStart.Month + 1;

            // Ensure quantity is between 0 and 3 (max months in a quarter)
            return Math.Max(Math.Min((decimal)months, 3.0m), 0m);
        }

        private DateTime DetermineStartOfQuarterForPeriod(string fiscalPeriodString)
        {
            // Parse fiscal period string like "Fiscal 25/26 Quarter 4"
            // Extract the year and quarter
            if (string.IsNullOrEmpty(fiscalPeriodString) || !fiscalPeriodString.StartsWith("Fiscal"))
                throw new ArgumentException("Invalid fiscal period string format");

            string[] parts = fiscalPeriodString.Split(' ');
            if (parts.Length < 4)
                throw new ArgumentException("Invalid fiscal period string format");

            string[] years = parts[1].Split('/');
            if (!int.TryParse(years[1], out int year))
                throw new ArgumentException("Cannot parse year from fiscal period string");

            int fullYear = 2000 + year; // Convert "26" to 2026

            string quarterPart = parts[3]; // The quarter number is the 4th element
            if (!int.TryParse(quarterPart, out int quarter))
                throw new ArgumentException("Cannot parse quarter from fiscal period string");

            // Fiscal quarters: Q1=Apr-Jun, Q2=Jul-Sep, Q3=Oct-Dec, Q4=Jan-Mar
            return quarter switch
            {
                1 => new DateTime(fullYear - 1, 4, 1),  // April of previous calendar year
                2 => new DateTime(fullYear - 1, 7, 1),  // July of previous calendar year
                3 => new DateTime(fullYear - 1, 10, 1), // October of previous calendar year
                4 => new DateTime(fullYear, 1, 1),      // January of current calendar year
                _ => throw new ArgumentException($"Invalid quarter number: {quarter}")
            };
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

        public IEnumerable<FiscalHistory> GetPreviousQuarterChargeHistory(string quarter = "")
        {
            try
            {
                string previousQuarterString = quarter;
                if (string.IsNullOrEmpty(previousQuarterString)) 
                    previousQuarterString = GetPreviousQuarterString();

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
            newBill.BillingCycle = bill.BillingCycle;
            newBill.ClientAccountId = bill.ClientAccountId;
            //    newBill.MostRecentActiveFiscalPeriod = bill.MostRecentActiveFiscalPeriod;
            newBill.CurrentFiscalPeriodId = bill.CurrentFiscalPeriodId;
            newBill.IdirOrUrl = bill.IdirOrUrl;
            newBill.StartDate = bill.StartDate;
            newBill.IsActive = bill.IsActive;
            newBill.TicketNumberAndRequester = bill.TicketNumberAndRequester;
            newBill.Notes = bill.Notes;
            newBill.EndDate = bill.EndDate;
            newBill.DateModified = bill.DateModified;
            newBill.CreatedBy = bill.CreatedBy;

            // Calculate the correct quantity based on the fiscal period - only for month-based services
            FiscalPeriod? fiscalPeriod = _fiscalPeriodRepository.GetFiscalPeriodById(bill.CurrentFiscalPeriodId);
            if (fiscalPeriod != null && bill.ServiceCategory != null && 
                !string.IsNullOrEmpty(bill.ServiceCategory.UOM) && 
                bill.ServiceCategory.UOM.Equals("month", StringComparison.OrdinalIgnoreCase))
            {
                DateTime periodStart = DetermineStartOfQuarterForPeriod(fiscalPeriod.Period);
                DateTime periodEnd = DetermineEndOfQuarter(periodStart);
                newBill.Quantity = CalculateQuantityForQuarter(bill, periodStart, periodEnd);
                _logger.LogInformation($"Creating bill in {fiscalPeriod.Period} with calculated quantity: {newBill.Quantity} (month-based service)");
            }
            else
            {
                newBill.Quantity = bill.Quantity;
                if (fiscalPeriod != null)
                {
                    _logger.LogInformation($"Creating bill in {fiscalPeriod.Period} with user-provided quantity: {newBill.Quantity} (non-month UOM)");
                }
            }

            newBill.Amount = bill.Amount;

            await _billingContext.AddAsync(newBill);
            await _billingContext.SaveChangesAsync();
            return newBill.Id;
        }

        public async Task Update(Bill editedBill, string userName = "system")
        {
            // Get the original bill from database to compare dates
            Bill? originalBill = _billingContext.Bills.AsNoTracking()
                .Include(b => b.MostRecentActiveFiscalPeriod)
                .Include(b => b.ServiceCategory)
                .FirstOrDefault(b => b.Id == editedBill.Id);

            if (originalBill != null)
            {
                // Check if the bill is in the current fiscal period
                string currentQuarterString = DetermineCurrentQuarter();
                FiscalPeriod? currentFiscalPeriod = _fiscalPeriodRepository.GetFiscalPeriodByString(currentQuarterString);
                DateTime currentQuarterStart = DetermineStartOfCurrentQuarter();

                if (currentFiscalPeriod != null)
                {
                    // Check if start or end dates changed
                    bool datesChanged = originalBill.StartDate != editedBill.StartDate || 
                                       originalBill.EndDate != editedBill.EndDate;

                    if (datesChanged)
                    {
                        // Check if bill is currently in the current quarter
                        if (editedBill.CurrentFiscalPeriodId == currentFiscalPeriod.Id)
                        {
                            // Only recalculate quantity for month-based services
                            if (editedBill.ServiceCategory != null && 
                                !string.IsNullOrEmpty(editedBill.ServiceCategory.UOM) && 
                                editedBill.ServiceCategory.UOM.Equals("month", StringComparison.OrdinalIgnoreCase))
                            {
                                // Recalculate quantity for the current quarter
                                DateTime currentQuarterEnd = DetermineEndOfQuarter(currentQuarterStart);
                                decimal newQuantity = CalculateQuantityForQuarter(editedBill, currentQuarterStart, currentQuarterEnd);

                                _logger.LogInformation($"Bill {editedBill.Id} dates changed. Recalculating quantity for current quarter: {newQuantity} (month-based service)");
                                editedBill.Quantity = newQuantity;

                                // Recalculate amount based on new quantity
                                if (!String.IsNullOrEmpty(editedBill.ServiceCategory.Costs))
                                {
                                    if (decimal.TryParse(editedBill.ServiceCategory.Costs, out decimal unitPrice))
                                    {
                                        editedBill.Amount = unitPrice * newQuantity;
                                    }
                                }
                            }
                            else
                            {
                                _logger.LogInformation($"Bill {editedBill.Id} dates changed, but UOM is not month-based. Quantity unchanged: {editedBill.Quantity}");
                            }
                        }
                        // Check if bill is in a previous quarter but now extends into current quarter
                        else if (editedBill.CurrentFiscalPeriodId != currentFiscalPeriod.Id)
                        {
                            // Check if the bill now extends into or past the current quarter
                            bool shouldPromote = (editedBill.EndDate == null || editedBill.EndDate.Value >= currentQuarterStart);

                            if (shouldPromote)
                            {
                                _logger.LogInformation($"Bill {editedBill.Id} end date changed. Bill now extends into current quarter. Promoting from period {editedBill.CurrentFiscalPeriodId} to {currentFiscalPeriod.Id}");

                                // Before promoting, ensure the ServiceCategory is loaded
                                if (editedBill.ServiceCategory == null && originalBill.ServiceCategory != null)
                                {
                                    editedBill.ServiceCategory = originalBill.ServiceCategory;
                                }

                                // Save current changes first
                                EntityEntry? tempEntry = await _changeLogRepository.MakeChangeLogAndReturnEntry(editedBill, userName);
                                Bill? tempBill = tempEntry?.Entity as Bill;
                                if (tempBill != null)
                                {
                                    _billingContext.Update(tempBill);
                                    await _billingContext.SaveChangesAsync();
                                }

                                // Reload the bill to get the saved version
                                Bill? savedBill = GetBill(editedBill.Id);
                                if (savedBill != null)
                                {
                                    await PromoteCharge(savedBill, currentFiscalPeriod, currentQuarterStart, true);
                                }

                                return; // Exit early since promotion handles the update
                            }
                        }
                    }
                }
            }

            EntityEntry? entry = await _changeLogRepository.MakeChangeLogAndReturnEntry(editedBill, userName);
            Bill? bill = entry?.Entity as Bill;
            if (bill != null)
            {
                _billingContext.Update(bill);
                await _billingContext.SaveChangesAsync();
            }
            else
                throw new Exception($"Something went wrong while trying to update Charge with Id {editedBill.Id}");
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