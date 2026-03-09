using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Service_Billing.Data;
using Service_Billing.Models;
using Service_Billing.Models.Repositories;
using Service_Billing.ViewModels;
using System.Collections.Immutable;
using System.Data;
using System.Reflection;

namespace Service_Billing.Controllers
{
    public class BillsController : Controller
    {
        private readonly IBillRepository _billRepository;
        private readonly IServiceCategoryRepository _categoryRepository;
        private readonly IClientAccountRepository _clientAccountRepository;
        private readonly IMinistryRepository _ministryRepository;
        private readonly GraphServiceClient _graphServiceClient;
        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;
        private readonly ILogger<BillsController> _logger;
        private readonly IAuthorizationService _authorizationService;
        private readonly IBusinessAreaRepository _businessAreaRepository;
        private readonly ServiceBillingContext _serviceBillingContext;
        private readonly IFiscalPeriodRepository _fiscalPeriodRepository;
        private readonly IFiscalHistoryRepository _fiscalHistoryRepository;
        private readonly IChangeLogRepository _changeLogRepository;

        public BillsController(ILogger<BillsController> logger,
            IBillRepository billRepository,
            IServiceCategoryRepository categoryRepository,
            IClientAccountRepository clientAccountRepository,
            IMinistryRepository ministryRepository,
            IAuthorizationService authorizationService,
            IBusinessAreaRepository businessAreaRepository,
            IFiscalPeriodRepository fiscalPeriodRepository,
            IFiscalHistoryRepository fiscalHistoryRepository,
            IChangeLogRepository changeLogRepository,
            ServiceBillingContext serviceBillingContext,
            IConfiguration configuration,
                            GraphServiceClient graphServiceClient,
                            MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
        {
            _graphServiceClient = graphServiceClient;
            _consentHandler = consentHandler;
            _billRepository = billRepository;
            _categoryRepository = categoryRepository;
            _clientAccountRepository = clientAccountRepository;
            _ministryRepository = ministryRepository;
            _logger = logger;
            _authorizationService = authorizationService;
            _businessAreaRepository = businessAreaRepository;
            _serviceBillingContext = serviceBillingContext;
            _fiscalPeriodRepository = fiscalPeriodRepository;
            _fiscalHistoryRepository = fiscalHistoryRepository;
            _changeLogRepository = changeLogRepository;
        }

        [Authorize]
        [Authorize(Roles = "GDXBillingService.FinancialOfficer, GDXBillingService.Owner, GDXBillingService.User")]
        public IActionResult Index(ChargeIndexSearchParamsModel searchModel)
        {
            IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();
            IEnumerable<BusinessArea> busareas = _businessAreaRepository.GetAll();
            IEnumerable<Ministry> ministries = _ministryRepository.GetAll();
            string previousQuarterString = _billRepository.GetPreviousQuarterString();
            string currentQuarterString = _billRepository.DetermineCurrentQuarter();
            ViewData["PreviousQuarterString"] = previousQuarterString;
            ViewData["CurrentQuarterString"] = currentQuarterString;
            IEnumerable<FiscalPeriod> fiscalPeriods = _fiscalPeriodRepository.GetAll().OrderByDescending(x => x.Period);
            List<string> fiscalPeriodsStrings = fiscalPeriods.Select(x => x.Period).ToList();
            fiscalPeriodsStrings.Remove(previousQuarterString);
            fiscalPeriodsStrings.Remove(currentQuarterString);
            ViewData["PreviousFiscals"] = fiscalPeriodsStrings.Distinct(); // not sure why there's duplicate entries in DEV...

            if (ministries != null && ministries.Any())
            {
                ViewData["Ministries"] = ministries;
            }
            if (categories != null && categories.Any())
            {
                if (!User.IsInRole("GDXBillingService.FinancialOfficer")
                    && User.IsInRole("GDXBillingService.Owner"))
                {
                    categories = categories.Where(c => GetUserOwnedServiceIds().Contains(c.ServiceId));
                }
                ViewBag.ServiceCategories = categories.ToList();
            }

            ViewBag.BusAreas = busareas.ToList();
            switch (searchModel?.QuarterFilter)
            {
                case "current":
                    ViewData["FiscalPeriod"] = _billRepository.DetermineCurrentQuarter();
                    break;
                case "previous":
                    ViewData["FiscalPeriod"] = previousQuarterString;
                    break;
                case "next":
                    ViewData["FiscalPeriod"] = _billRepository.DetermineCurrentQuarter(_billRepository.DetermineStartOfNextQuarter());
                    break;
                case "all":
                    ViewData["FiscalPeriod"] = "all";
                    break;
                default:
                    ViewData["FiscalPeriod"] = searchModel?.QuarterFilter;

                    break;
            }
            ViewData["searchModel"] = searchModel;

            return View();
        }

        [HttpPost]
        public ActionResult GetBillsTable(ChargeIndexSearchParamsModel searchModel)
        {
            var isMinistryUser = User.IsInRole("GDXBillingService.User");
            string? ministryUserName = string.Empty;
            if (isMinistryUser) ministryUserName = User?.FindFirst("name")?.Value;
            if (searchModel != null && !string.IsNullOrEmpty(searchModel.QuarterFilter) &&
                searchModel.QuarterFilter.StartsWith("Fiscal"))
                searchModel.QuarterString = searchModel.QuarterFilter;

            IEnumerable<Bill> bills = QueryForCharges(searchModel, !String.IsNullOrEmpty(ministryUserName) ? ministryUserName : String.Empty);
            int count = bills.Any() ? bills.Count() : 0;

            if (searchModel != null && !String.IsNullOrEmpty(searchModel.QuarterFilter))
            {
                switch (searchModel.QuarterFilter)
                {
                    case "current":
                        ViewData["FiscalPeriod"] = _billRepository.DetermineCurrentQuarter();
                        break;
                    case "previous":
                        searchModel.QuarterString = _billRepository.GetPreviousQuarterString();
                        ViewData["FiscalPeriod"] = searchModel.QuarterString;
                        ViewData["IsPrevious"] = "yes";
                        FiscalPeriod? previousFiscal = _fiscalPeriodRepository.GetFiscalPeriodByString(searchModel.QuarterString);
                        if (previousFiscal == null)
                            throw new Exception($"Could not find fiscal period from database matching '{searchModel.QuarterString}'");
                        else
                            ViewData["PreviousFiscalId"] = previousFiscal.Id;
                        break;
                    case "next":
                        ViewData["FiscalPeriod"] = _billRepository.DetermineCurrentQuarter(_billRepository.DetermineStartOfNextQuarter());
                        break;
                    case "all":
                        ViewData["FiscalPeriod"] = "all";
                        count = 0;
                        foreach (Bill bill in bills)
                        {
                            count++;
                            if (bill.PreviousFiscalRecords.Any())
                                count += bill.PreviousFiscalRecords.Where(x => x.FiscalPeriod.Id != bill.CurrentFiscalPeriodId).Count();
                        }
                        break;
                    default:
                        searchModel.QuarterString = searchModel.QuarterFilter;
                        ViewData["FiscalPeriod"] = searchModel.QuarterString;
                        break;
                }
            }
            else
            {
                ViewData["FiscalPeriod"] = _billRepository.DetermineCurrentQuarter();
            }
            ViewData["ChargesReturnedByQuery"] = count;

            return PartialView("ChargesTable", bills);
        }

        public ActionResult Details(int id, int? historyId, bool isNew = false, bool isEdited = false)
        {
            Bill? bill = _billRepository.GetBill(id);
            try
            {
                if (bill == null)
                {
                    return NotFound();
                }
                ClientAccount? account = _clientAccountRepository.GetClientAccount(bill.ClientAccountId);
                ServiceCategory? serviceCategory = _categoryRepository.GetById(bill.ServiceCategoryId);
                ViewData["clientAccount"] = account != null ? account : "";
                ViewData["serviceCategory"] = serviceCategory != null ? serviceCategory : "";
                ViewData["isNew"] = isNew;
                ViewData["isEdited"] = isEdited;
                if (historyId > 0)
                {
                    FiscalHistory? fiscalHistory = bill.PreviousFiscalRecords?.FirstOrDefault(x => x.Id == historyId);
                    if (fiscalHistory == null)
                    {
                        throw new Exception("Tried to view charge details with fiscal history Id present, but no fiscal history was found");
                    }
                    else
                    {
                        ViewData["historyData"] = fiscalHistory;
                        ViewData["periodString"] = fiscalHistory?.FiscalPeriod?.Period;
                    }
                }

                ViewData["ChangeLogs"] = _changeLogRepository.GetByEnityIdAndType(bill.Id, "charge");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return View(bill);
        }

        public ActionResult Edit(int id, int? historyId)
        {
            if (User.IsInRole("GDXBillingService.User"))
            {
                return View("Unauthorized");
            }

            Bill? bill = _billRepository.GetBill(id);
            _logger.LogInformation($"Editing Bill with ID: {id}");
            if (bill == null)
                _logger.LogWarning($"Bill with Id: {id} was not found in database");

            if (bill == null)
                return NotFound();

            EditChargeViewModel model;
            if (historyId != null)
            {
                FiscalHistory? fiscalHistory = bill.PreviousFiscalRecords?.FirstOrDefault(x => x.Id == historyId);
                model = new EditChargeViewModel(bill, fiscalHistory);
            }
            else
            {
                model = new EditChargeViewModel(bill, new FiscalHistory());
            }
            if (String.IsNullOrEmpty(bill.MostRecentActiveFiscalPeriod.Period) || String.IsNullOrEmpty(bill.BillingCycle))
                DetermineCurrentQuarter(bill, bill.DateCreated);

            model.Categories = _categoryRepository.GetAll();
            ServiceCategory? serviceCategory = _categoryRepository.GetById(bill.ServiceCategoryId);

            return View(model);
        }

        // POST: ClientAccountController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditChargeViewModel model)
        {
            try
            {
                // Exclude nested navigation properties.
                ModelState.Remove("Bill.ClientAccount.STOB");
                ModelState.Remove("Bill.ClientAccount.Project");
                ModelState.Remove("Bill.ClientAccount.Approver");
                ModelState.Remove("Bill.ClientAccount.ServiceLine");
                ModelState.Remove("Bill.ClientAccount.ClientNumber");
                ModelState.Remove("Bill.ClientAccount.OrganizationId");
                ModelState.Remove("Bill.ClientAccount.PrimaryContact");
                ModelState.Remove("Bill.ClientAccount.ExpenseAuthorityName");
                ModelState.Remove("Bill.ClientAccount.ResponsibilityCentre");
                ModelState.Remove("Bill.ServiceCategory.Name");
                ModelState.Remove("Bill.ServiceCategory.Description");

                // Get the original bill to check if StartDate is being changed
                Bill? originalBill = _billRepository.GetBill(model.Bill.Id);

                // Only validate StartDate if it's being changed from the original value
                if (originalBill != null && model.Bill.StartDate.HasValue && 
                    originalBill.StartDate.HasValue && 
                    model.Bill.StartDate.Value.Date != originalBill.StartDate.Value.Date)
                {
                    DateTimeOffset previousQuarterStart = GetPreviousQuarterStart();
                    DateTimeOffset nextQuarterStart = _billRepository.DetermineStartOfNextQuarter();
                    DateTimeOffset quarterAfterNextStart = GetQuarterAfterNext(nextQuarterStart);

                    // Check if the original charge has a StartDate before the previous quarter
                    // If so, it has historical records and the StartDate should not be changed at all
                    if (originalBill.StartDate.Value.Date < previousQuarterStart.Date)
                    {
                        ModelState.AddModelError("Bill.StartDate",
                            "This charge's start date has been recognized in historical fiscal periods and cannot be changed. " +
                            "Changing this date would corrupt historical billing records. " +
                            "Please contact a Service Billing administrator if you need to make corrections.");
                    }
                    // For recent charges (within editable range), validate the new StartDate
                    else
                    {
                        // Check if new date is too far in the past (before previous quarter)
                        if (model.Bill.StartDate.Value.Date < previousQuarterStart.Date)
                        {
                            string previousQuarter = _billRepository.DetermineCurrentQuarter(previousQuarterStart.DateTime);

                            ModelState.AddModelError("Bill.StartDate",
                                $"Start date cannot be earlier than the previous fiscal period ({previousQuarter}). " +
                                $"If you need to backdate this charge further, please contact a Service Billing administrator.");
                        }

                        // Check if date is too far in the future (beyond next quarter)
                        if (model.Bill.StartDate.Value.Date >= quarterAfterNextStart.Date)
                        {
                            string currentQuarter = _billRepository.DetermineCurrentQuarter();
                            string nextQuarter = _billRepository.DetermineCurrentQuarter(nextQuarterStart.DateTime);

                            ModelState.AddModelError("Bill.StartDate",
                                $"Start date cannot be beyond the next fiscal period ({nextQuarter}). Current fiscal period is {currentQuarter}. " +
                                $"Please contact an administrator if you need to set a future start date.");
                        }
                    }
                }

                // Validate that EndDate is not before StartDate
                if (model.Bill.StartDate.HasValue && model.Bill.EndDate.HasValue && model.Bill.EndDate.Value < model.Bill.StartDate.Value)
                {
                    ModelState.AddModelError("Bill.EndDate", "End date cannot be before the start date.");
                }

                // Validate the model state first
                if (!ModelState.IsValid)
                {
                    // Re-populate the categories
                    model.Categories = _categoryRepository.GetAll();
                    return View(model);
                }

                model.Bill.ServiceCategory = _categoryRepository.GetById(model.Bill.ServiceCategoryId);
                model.Bill.DateModified = GetUserLocalTime();
                string user = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "NAME NOT DETERMINED";

                await _billRepository.Update(model.Bill, user);
                if (model.FiscalHistory != null && model.FiscalHistory.Id > 0)
                {
                    FiscalHistory? fh = _fiscalHistoryRepository.GetFiscalHistoryById(model.FiscalHistory.Id);
                    if (fh != null)
                    {
                        fh.Notes = model.FiscalHistory.Notes;
                        await _fiscalHistoryRepository.UpdateFiscalHistory(fh);
                    }
                    else
                    {
                        _logger.LogError($"Attempted to update FiscalHistory with ID: {model.FiscalHistory.Id}, but no matching FiscalHistory was found in DB ");
                    }
                }

                return RedirectToAction("Details", new { model.Bill.Id, isEdited = true, historyId = model.FiscalHistory?.Id });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Bill failed to update. Exception: {ex.InnerException}");
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists, " +
                    "see your system administrator.");

                // Re-populate the categories
                model.Categories = _categoryRepository.GetAll();
                return View(model);
            }
        }


        [HttpGet]
        public async Task<ActionResult> Create(int accountId)
        {
            IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();
            ViewData["Categories"] = categories;
            ViewData["CurrentUser"] = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "";
            Bill bill = new Bill();

            /* offloading this bit to Bill's constructor
             DateTime utcDate = DateTime.UtcNow;
             TimeZoneInfo pacificZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"); // Handles both PST and PDT
             DateTime pacificTime = TimeZoneInfo.ConvertTimeFromUtc(utcDate, pacificZone);
             bill.StartDate = pacificTime;
             bill.DateCreated = pacificTime;
             */

            DetermineCurrentQuarter(bill, bill.DateCreated);
            if (accountId > 0)
            {
                ClientAccount? account = _clientAccountRepository.GetClientAccount(accountId);
                if (account != null)
                {
                    bill.ClientAccountId = accountId;
                    bill.ClientAccount = account;
                    bill.ClientAccount.Name = account.Name;
                }
            }

            return View(bill);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Bill bill)
        {
            try
            {
                // Manually validate the StartDate - allow previous quarter, current quarter, and next quarter only
                if (bill.StartDate.HasValue)
                {
                    DateTimeOffset previousQuarterStart = GetPreviousQuarterStart();
                    DateTimeOffset nextQuarterStart = _billRepository.DetermineStartOfNextQuarter();
                    DateTimeOffset quarterAfterNextStart = GetQuarterAfterNext(nextQuarterStart);

                    // Check if date is too far in the past (before previous quarter)
                    if (bill.StartDate.Value.Date < previousQuarterStart.Date)
                    {
                        string previousQuarter = _billRepository.DetermineCurrentQuarter(previousQuarterStart.DateTime);

                        ModelState.AddModelError("StartDate",
                            $"Start date cannot be earlier than the previous fiscal period ({previousQuarter}). " +
                            $"If you need to create a charge with an earlier start date, please contact a Service Billing administrator.");

                        // Re-populate the view data
                        IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();
                        ViewData["Categories"] = categories;
                        ViewData["CurrentUser"] = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "";

                        if (_clientAccountRepository.GetClientAccount(bill.ClientAccountId) != null)
                        {
                            bill.ClientAccount = _clientAccountRepository.GetClientAccount(bill.ClientAccountId);
                        }

                        return View(bill);
                    }

                    // Check if date is too far in the future (beyond next quarter)
                    if (bill.StartDate.Value.Date >= quarterAfterNextStart.Date)
                    {
                        string currentQuarter = _billRepository.DetermineCurrentQuarter();
                        string nextQuarter = _billRepository.DetermineCurrentQuarter(nextQuarterStart.DateTime);

                        ModelState.AddModelError("StartDate",
                            $"Start date cannot be beyond the next fiscal period ({nextQuarter}). Current fiscal period is {currentQuarter}. " +
                            $"Please contact an administrator if you need to create a charge for a future period.");

                        // Re-populate the view data
                        IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();
                        ViewData["Categories"] = categories;
                        ViewData["CurrentUser"] = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "";

                        if (_clientAccountRepository.GetClientAccount(bill.ClientAccountId) != null)
                        {
                            bill.ClientAccount = _clientAccountRepository.GetClientAccount(bill.ClientAccountId);
                        }

                        return View(bill);
                    }
                }

                // Validate that EndDate is not before StartDate
                if (bill.StartDate.HasValue && bill.EndDate.HasValue && bill.EndDate.Value < bill.StartDate.Value)
                {
                    ModelState.AddModelError("EndDate", "End date cannot be before the start date.");

                    // Re-populate the view data
                    IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();
                    ViewData["Categories"] = categories;
                    ViewData["CurrentUser"] = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "";

                    if (_clientAccountRepository.GetClientAccount(bill.ClientAccountId) != null)
                    {
                        bill.ClientAccount = _clientAccountRepository.GetClientAccount(bill.ClientAccountId);
                    }

                    return View(bill);
                }

                // Continue with normal processing...
                ClientAccount? account = _clientAccountRepository.GetClientAccount(bill.ClientAccountId);
                ServiceCategory? category = _categoryRepository.GetById(bill.ServiceCategoryId);
                if (account == null || category == null)
                {
                    throw new Exception("Could not find either a client account or service category when attempting to create new charge entry.");
                }
                if (string.IsNullOrEmpty(bill.CreatedBy))
                    bill.CreatedBy = await GetMyName();

                bill.ClientAccount = account;
                bill.ServiceCategory = category;

                // Determine which fiscal period this charge belongs to based on StartDate
                DetermineCurrentQuarter(bill, bill.StartDate);

                // Ensure the fiscal period exists for the charge's start date
                FiscalPeriod? fiscalPeriod = await EnsureFiscalPeriodExists(bill.StartDate);

                if (fiscalPeriod == null)
                {
                    throw new Exception($"Unable to create or retrieve fiscal period for start date {bill.StartDate}");
                }

                // Set the bill's CurrentFiscalPeriodId to match the fiscal period for its start date
                bill.CurrentFiscalPeriodId = fiscalPeriod.Id;
                bill.MostRecentActiveFiscalPeriod = fiscalPeriod;

                _logger.LogInformation($"New charge is valid. Assigned to fiscal period: {fiscalPeriod.Period} (ID: {fiscalPeriod.Id})");

                int billId = await _billRepository.CreateBill(bill);
                bill = _billRepository.GetBill(billId);

                // Determine if this charge should be promoted to the current quarter
                DateTimeOffset currentQuarterStart = _billRepository.DetermineStartOfCurrentQuarter();
                bool shouldPromoteToCurrentQuarter = false;

                if (bill?.StartDate != null && bill.StartDate.Value.Date < currentQuarterStart.Date)
                {
                    // Check if the charge ended before the current quarter started
                    if (bill.EndDate.HasValue && bill.EndDate.Value.Date < currentQuarterStart.Date)
                    {
                        // Charge ended before current quarter - do NOT promote
                        // Create FiscalHistory for this charge since it stays in the previous quarter
                        if (bill.ServiceCategory != null && !String.IsNullOrEmpty(bill.ServiceCategory.Costs))
                        {
                            if (decimal.TryParse(bill.ServiceCategory.Costs, out decimal unitPrice))
                            {
                                FiscalHistory historicalRecord = new FiscalHistory(
                                    bill.Id,
                                    fiscalPeriod.Id,
                                    unitPrice,
                                    bill.Quantity,
                                    bill.Notes
                                );
                                _fiscalHistoryRepository.SaveFiscalHistoryInfo(historicalRecord);
                                _logger.LogInformation($"Created FiscalHistory record for charge {bill.Id} in {fiscalPeriod.Period} (charge ended in this quarter)");
                            }
                        }

                        _logger.LogInformation($"Charge {bill.Id} created with StartDate={bill.StartDate.Value.Date:yyyy-MM-dd} and EndDate={bill.EndDate.Value.Date:yyyy-MM-dd}. " +
                            $"Both dates are in {fiscalPeriod.Period}. Charge will remain in {fiscalPeriod.Period} and will NOT be promoted to current quarter.");
                        shouldPromoteToCurrentQuarter = false;
                    }
                    else
                    {
                        // Charge is either ongoing (no EndDate) or extends into/past current quarter - DO promote
                        // PromoteCharge will handle creating the FiscalHistory for the previous quarter
                        shouldPromoteToCurrentQuarter = true;
                    }

                    if (shouldPromoteToCurrentQuarter)
                    {
                        FiscalPeriod? currentFiscalPeriod = _fiscalPeriodRepository.GetFiscalPeriodByString(_billRepository.DetermineCurrentQuarter());
                        if (currentFiscalPeriod != null)
                        {
                            await _billRepository.PromoteCharge(bill, currentFiscalPeriod);
                            _logger.LogInformation($"Charge {bill.Id} promoted from {fiscalPeriod.Period} to {currentFiscalPeriod.Period}");
                        }
                        else
                        {
                            _logger.LogError($"Could not find current fiscal period to promote charge {bill.Id}");
                        }
                    }
                }
                else if (bill?.StartDate != null && bill.StartDate.Value >= currentQuarterStart)
                {
                    _logger.LogInformation($"Charge {bill.Id} created with StartDate in current or future quarter ({fiscalPeriod.Period}). No promotion needed.");
                }

                return RedirectToAction($"Details", new { id = bill?.Id, historyId = string.Empty, isNew = true });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Creating a new charge failed to write to database. Exception: {ex.InnerException}");
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");

                // Re-populate the view data
                IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();
                ViewData["Categories"] = categories;
                ViewData["CurrentUser"] = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "";

                return View(bill);
            }
        }

        // Helper method to ensure fiscal period exists, creating it if necessary
        private async Task<FiscalPeriod?> EnsureFiscalPeriodExists(DateTimeOffset? startDate)
        {
            if (startDate == null)
            {
                startDate = DateTimeOffset.Now;
            }

            // Determine the fiscal period string for the start date
            string fiscalPeriodString = _billRepository.DetermineCurrentQuarter(startDate.Value.DateTime);

            _logger.LogInformation($"Looking for fiscal period: {fiscalPeriodString}");

            // Try to get the fiscal period from the database
            FiscalPeriod? fiscalPeriod = _fiscalPeriodRepository.GetFiscalPeriodByString(fiscalPeriodString);

            // If it doesn't exist, create it
            if (fiscalPeriod == null)
            {
                _logger.LogInformation($"Fiscal period {fiscalPeriodString} does not exist. Creating it now.");

                fiscalPeriod = new FiscalPeriod(fiscalPeriodString);

                // Save the new fiscal period to the database
                _fiscalPeriodRepository.SaveFiscalPeriod(fiscalPeriod);

                // Retrieve it again to ensure we have the generated ID
                fiscalPeriod = _fiscalPeriodRepository.GetFiscalPeriodByString(fiscalPeriodString);

                if (fiscalPeriod != null)
                {
                    _logger.LogInformation($"Created fiscal period {fiscalPeriodString} with ID: {fiscalPeriod.Id}");
                }
                else
                {
                    _logger.LogError($"Failed to create fiscal period {fiscalPeriodString}");
                }
            }
            else
            {
                _logger.LogInformation($"Found existing fiscal period {fiscalPeriodString} with ID: {fiscalPeriod.Id}");
            }

            return fiscalPeriod;
        }

        // Helper method to get the start of the previous quarter
        private DateTimeOffset GetPreviousQuarterStart()
        {
            DateTime currentQuarterStart = _billRepository.DetermineStartOfCurrentQuarter();

            switch (currentQuarterStart.Month)
            {
                case 4:  // Q1 -> go back to Q4 (January of same year)
                    return new DateTimeOffset(currentQuarterStart.Year, 1, 1, 0, 0, 0, TimeSpan.FromHours(-8));
                case 7:  // Q2 -> go back to Q1 (April of same year)
                    return new DateTimeOffset(currentQuarterStart.Year, 4, 1, 0, 0, 0, TimeSpan.FromHours(-8));
                case 10: // Q3 -> go back to Q2 (July of same year)
                    return new DateTimeOffset(currentQuarterStart.Year, 7, 1, 0, 0, 0, TimeSpan.FromHours(-8));
                case 1:  // Q4 -> go back to Q3 (October of previous year)
                    return new DateTimeOffset(currentQuarterStart.Year - 1, 10, 1, 0, 0, 0, TimeSpan.FromHours(-8));
                default:
                    return new DateTimeOffset(currentQuarterStart, TimeSpan.FromHours(-8));
            }
        }

        // Helper method for calculating quarter after next
        private DateTimeOffset GetQuarterAfterNext(DateTimeOffset nextQuarterStart)
        {
            int month = nextQuarterStart.Month;
            int year = nextQuarterStart.Year;

            int quarterAfterNextMonth = month switch
            {
                4 => 7,   // Q1 -> Q2
                7 => 10,  // Q2 -> Q3
                10 => 1,  // Q3 -> Q4 (next calendar year)
                1 => 4,   // Q4 -> Q1
                _ => throw new InvalidOperationException($"Invalid quarter start month: {month}")
            };

            if (month == 10)
            {
                year++;
            }

            return new DateTimeOffset(year, quarterAfterNextMonth, 1, 0, 0, 0, nextQuarterStart.Offset);
        }

        [HttpGet]
        public ActionResult? GetBillAmount(short? serviceId, decimal? quantity, string? startDate, string? endDate, bool quantityChanged = false)
        {
            try
            {
                if (serviceId == null || quantity == null)
                    return null;
                ServiceCategory? category = _categoryRepository.GetById(serviceId);
                if (category == null)
                {
                    return null;
                }

                string quantityExplanation = "";

                // Calculate quantity based on start and end dates for month-based services
                if (category?.UOM?.ToLower() == "month" && !quantityChanged)
                {
                    if (!String.IsNullOrEmpty(startDate) && DateTimeOffset.TryParse(startDate, out DateTimeOffset start))
                    {
                        DateTimeOffset currentQuarterStart = _billRepository.DetermineStartOfCurrentQuarter();
                        DateTimeOffset currentQuarterEnd = _billRepository.DetermineEndOfQuarter(currentQuarterStart.Date);
                        DateTimeOffset previousQuarterStart = GetPreviousQuarterStart();
                        DateTimeOffset nextQuarterStart = _billRepository.DetermineStartOfNextQuarter();
                        DateTimeOffset nextQuarterEnd = _billRepository.DetermineEndOfQuarter(nextQuarterStart.DateTime);

                        DateTimeOffset? end = null;
                        if (!String.IsNullOrEmpty(endDate) && DateTimeOffset.TryParse(endDate, out DateTimeOffset parsedEnd))
                        {
                            end = parsedEnd;
                        }

                        // Determine which quarter to calculate for
                        if (start < currentQuarterStart)
                        {
                            // Start date is in previous quarter
                            DateTimeOffset relevantEnd = end.HasValue && end.Value < currentQuarterStart ? end.Value : currentQuarterStart.AddDays(-1);
                            int months = ((relevantEnd.Year - start.Year) * 12) + relevantEnd.Month - start.Month + 1;
                            quantity = Math.Max(months, 0);
                            
                            string previousQuarter = _billRepository.DetermineCurrentQuarter(previousQuarterStart.DateTime);
                            quantityExplanation = end.HasValue && end.Value < currentQuarterStart 
                                ? $"Billing for {quantity} month(s) in {previousQuarter} (from {start:MMM d} to {relevantEnd:MMM d, yyyy})"
                                : $"Billing for {quantity} month(s) in {previousQuarter} (charge will be promoted to current quarter)";
                        }
                        else if (start >= nextQuarterStart)
                        {
                            // Start date is in next quarter
                            DateTimeOffset relevantEnd = end.HasValue && end.Value <= nextQuarterEnd ? end.Value : nextQuarterEnd;
                            int months = ((relevantEnd.Year - start.Year) * 12) + relevantEnd.Month - start.Month + 1;
                            quantity = Math.Max(Math.Min(months, 3), 0);
                            
                            string nextQuarter = _billRepository.DetermineCurrentQuarter(nextQuarterStart.DateTime);
                            quantityExplanation = end.HasValue 
                                ? $"Billing for {quantity} month(s) in {nextQuarter} (from {start:MMM d} to {end.Value:MMM d, yyyy})"
                                : $"Billing for {quantity} month(s) in {nextQuarter}";
                        }
                        else
                        {
                            // Start date is in current quarter
                            DateTimeOffset relevantEnd = end.HasValue && end.Value <= currentQuarterEnd ? end.Value : currentQuarterEnd;
                            int months = ((relevantEnd.Year - start.Year) * 12) + relevantEnd.Month - start.Month + 1;
                            quantity = Math.Max(months, 0);
                            
                            string currentQuarter = _billRepository.DetermineCurrentQuarter();
                            quantityExplanation = end.HasValue && end.Value <= currentQuarterEnd
                                ? $"Billing for {quantity} month(s) in {currentQuarter} (from {start:MMM d} to {end.Value:MMM d, yyyy})"
                                : $"Billing for {quantity} month(s) in {currentQuarter} (started {start:MMM d, yyyy})";
                        }

                        _logger.LogInformation($"Calculated quantity: {quantity} months. {quantityExplanation}");
                    }
                }
                else if (category?.UOM?.ToLower() != "month")
                {
                    quantityExplanation = $"Unit of measure is {category?.UOM}, not month-based";
                }

                decimal newAmount;
                string cost = !String.IsNullOrEmpty(category?.Costs) ? category.Costs : "0";
                if (!string.IsNullOrEmpty(cost) && cost.Contains('$'))
                {
                    cost = cost.Replace('$', ' ');
                    cost = cost.Trim();
                }
                if (!decimal.TryParse(cost, out newAmount))
                {
                    newAmount = 0;
                }
                if (category?.ServiceId == 5)
                    newAmount = 85;
                string? UOM = !string.IsNullOrEmpty(category?.UOM) ? category.UOM : "n/a";

                var result = new
                {
                    serviceCategory = !String.IsNullOrEmpty(category?.Name) ? category.Name : "NoCategoryName",
                    amount = newAmount * quantity,
                    uom = UOM,
                    quantity = quantity,
                    quantityExplanation = quantityExplanation
                };

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new JsonResult(ex.Message);
            }
        }

        [HttpGet]
        public ActionResult GetClients()
        {
            try
            {
                return new JsonResult(from a in _clientAccountRepository.GetAll() select new { a.Id, a.Name });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}", ex);
                return new JsonResult(ex.Message);
            }
        }

        public void DetermineCurrentQuarter(Bill bill, DateTimeOffset? date = null)
        {
            DateTimeOffset today = DateTimeOffset.Now;
            if (date != null)
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
                    bill.BillingCycle = new DateTimeOffset(today.Year, 4, 1, 0, 0, 0, today.Offset).ToString("yyyy-MM-dd");
                    break;
                case 7:
                case 8:
                case 9:
                    quarter = "Quarter 2";
                    bill.BillingCycle = new DateTimeOffset(today.Year, 7, 1, 0, 0, 0, today.Offset).ToString("yyyy-MM-dd");
                    break;
                case 10:
                case 11:
                case 12:
                    quarter = "Quarter 3";
                    bill.BillingCycle = new DateTimeOffset(today.Year, 10, 1, 0, 0, 0, today.Offset).ToString("yyyy-MM-dd");
                    break;
                case 1:
                case 2:
                case 3:
                    quarter = "Quarter 4";
                    bill.BillingCycle = new DateTimeOffset(today.Year, 1, 1, 0, 0, 0, today.Offset).ToString("yyyy-MM-dd");
                    string fiscalPeriodString = $"Fiscal {(today.Year - 1).ToString().Substring(2)}/{year1.Substring(2)} {quarter}";
                    FiscalPeriod? CurrentFiscalPeriod = _fiscalPeriodRepository.GetFiscalPeriodByString(fiscalPeriodString);
                    if (CurrentFiscalPeriod == null)
                    {
                        _logger.LogError($"No existing Fiscal Period entity found for current Fiscal {(today.Year - 1).ToString().Substring(2)}/{year1.Substring(2)} {quarter}");
                    }
                    else
                    {
                        bill.CurrentFiscalPeriodId = CurrentFiscalPeriod.Id;
                    }

                    return;
            }
            FiscalPeriod? CurrentFiscal = _fiscalPeriodRepository.GetFiscalPeriodByString($"Fiscal {year1.Substring(2)}/{year2.Substring(2)} {quarter}");
            if (CurrentFiscal == null)
                _logger.LogError($"could not find a fiscal period entity for \"Fiscal {year1.Substring(2)}/{year2.Substring(2)} {quarter}\" ");
            else
                bill.CurrentFiscalPeriodId = CurrentFiscal.Id;
        }

        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public async Task<String> GetMyName()
        {
            try
            {
                var myName = await _graphServiceClient.Me.Request()
                .Select("displayName")
                .GetAsync();

                return myName.DisplayName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return "Could not get user's Graph name";
            }
        }

        // This is great! Much faster than getting all records, then filtering. 
        private IEnumerable<Bill> QueryForCharges(ChargeIndexSearchParamsModel? searchParams, string ministryUserName = "")
        {
            try
            {
                IQueryable<Bill> query = _serviceBillingContext.Bills
                    .Include(b => b.ClientAccount).ThenInclude(c => c.Contacts).ThenInclude(p => p.Person)
                    .Include(b => b.ServiceCategory)
                    .Include(b => b.PreviousFiscalRecords)
                    .ThenInclude(r => r.FiscalPeriod);

                //Todo: Give this some more thought. Can probably simplify this logic.
                bool restrictToOwnedServices = (!User.IsInRole("GDXBillingService.FinancialOfficer")
                    && User.IsInRole("GDXBillingService.Owner"));
                bool restrictToUserContact = (!User.IsInRole("GDXBillingService.FinancialOfficer")
                    && !User.IsInRole("GDXBillingService.Owner"));

                List<FiscalHistory> previousQuarterChargeIds = new List<FiscalHistory>();
                bool isNextQuarter = false;
                DateTime nextQuarterStart = DateTime.MinValue;
                DateTime nextQuarterEnd = DateTime.MinValue;

                switch (searchParams?.QuarterFilter)
                {
                    case "current":
                    case "Current Quarter":

                        string fiscalPeriodString = _billRepository.DetermineCurrentQuarter();
                        FiscalPeriod? fiscalPeriod = _fiscalPeriodRepository.GetFiscalPeriodByString(fiscalPeriodString);
                        if (fiscalPeriod == null)
                        {
                            throw new Exception($"No Fiscal Period entity was found that matches \"{fiscalPeriodString}\"");
                        }
                        query = query.Where(b => b.CurrentFiscalPeriodId == fiscalPeriod.Id);
                        query = query.Where(b => b.IsActive);
                        query = query.Where(b => b.ClientAccount.IsActive);
                        break;
                    case "previous":
                        string previousQuarterString = _billRepository.GetPreviousQuarterString();
                        FiscalPeriod? previousFiscalPeriod = _fiscalPeriodRepository.GetFiscalPeriodByString(previousQuarterString);

                        if (previousFiscalPeriod != null)
                        {
                            // Get bills that have FiscalHistory for the previous quarter (promoted FROM previous quarter)
                            previousQuarterChargeIds = _billRepository.GetPreviousQuarterChargeHistory().ToList();
                            List<int> billIdsWithHistory = previousQuarterChargeIds.Select(x => x.BillId).ToList();

                            // Include bills that are currently assigned to the previous quarter (stayed IN previous quarter)
                            query = query.Where(b => billIdsWithHistory.Contains(b.Id) || b.CurrentFiscalPeriodId == previousFiscalPeriod.Id);
                        }
                        else
                        {
                            throw new Exception($"Could not find fiscal period for previous quarter: {previousQuarterString}");
                        }
                        break;
                    case "next":
                        List<int> idsOfFixedServices = _billRepository.GetFixedServices();
                        nextQuarterStart = _billRepository.DetermineStartOfNextQuarter();
                        nextQuarterEnd = _billRepository.DetermineEndOfQuarter(nextQuarterStart);
                        isNextQuarter = true;
                        query = query.Where(b => idsOfFixedServices.Contains(b.ServiceCategoryId) && (b.EndDate == null || b.EndDate >= nextQuarterStart));
                        query = query.Where(b => b.IsActive);
                        query = query.Where(b => b.ClientAccount.IsActive);
                        ViewData["FiscalPeriod"] = _billRepository.DetermineCurrentQuarter(_billRepository.DetermineStartOfNextQuarter());
                        break;
                    case "all":
                        // Just break. Effectively it's just one less Where clause
                        break;
                    default: //get charges from previous fiscal history or bills currently in that quarter
                        if (!string.IsNullOrEmpty(searchParams.QuarterString))
                        {
                            FiscalPeriod? historicalFiscalPeriod = _fiscalPeriodRepository.GetFiscalPeriodByString(searchParams.QuarterString);

                            if (historicalFiscalPeriod != null)
                            {
                                // Get bills that have FiscalHistory for this quarter (promoted FROM this quarter)
                                previousQuarterChargeIds = _billRepository.GetPreviousQuarterChargeHistory(searchParams.QuarterString).ToList();
                                List<int> billIdsWithHistory = previousQuarterChargeIds.Select(x => x.BillId).ToList();

                                // Include bills that are currently assigned to this quarter (stayed IN this quarter)
                                query = query.Where(b => billIdsWithHistory.Contains(b.Id) || b.CurrentFiscalPeriodId == historicalFiscalPeriod.Id);
                            }
                            else
                            {
                                _logger.LogWarning($"Could not find fiscal period for quarter string: {searchParams.QuarterString}");
                            }
                        }
                        break;
                }
                if (!String.IsNullOrEmpty(searchParams?.QuarterFilter) && (searchParams?.QuarterFilter == "current" || searchParams?.QuarterFilter == "next"))
                {
                    IEnumerable<ClientAccount> inactiveAccounts = _clientAccountRepository.GetInactiveAccounts();
                    query = query.Where(b => !inactiveAccounts.Select(a => a.Id).Contains(b.ClientAccountId));
                }
                if (!string.IsNullOrEmpty(searchParams?.TitleFilter))
                    query = query.Where(x => x.Title.ToLower().Contains(searchParams.TitleFilter.ToLower()));
                if (restrictToOwnedServices)
                { //user is service owner, and we should only show services for charges they own
                    List<int> serviceIds = GetUserOwnedServiceIds();
                    query = query.Where(b => serviceIds.Contains(b.ServiceCategoryId));
                }

                if (searchParams?.MinistryFilter > 0)
                {
                    query = query.Where(x => x.ClientAccount.OrganizationId != null && x.ClientAccount.OrganizationId == searchParams.MinistryFilter);
                }
                if (!string.IsNullOrEmpty(searchParams?.TitleFilter))
                    query = query.Where(x => !String.IsNullOrEmpty(x.Title) && x.Title.ToLower().Contains(searchParams.TitleFilter.ToLower()));
                if (searchParams?.BusAreaFilter > 0)
                {
                    query = query.Where(x => x.ServiceCategory.BusAreaId == searchParams.BusAreaFilter);
                }
                if (searchParams?.CategoryFilter != null && searchParams?.CategoryFilter.Count > 0)
                {
                    query = query.Where(b => searchParams.CategoryFilter.Contains(b.ServiceCategoryId));
                }
                if (!string.IsNullOrEmpty(searchParams?.Keyword))
                    query = query.Where(x => (!String.IsNullOrEmpty(x.Title) && x.Title.ToLower().Contains(searchParams.Keyword.ToLower())) ||
                       (!String.IsNullOrEmpty(x.IdirOrUrl) && x.IdirOrUrl.ToLower().Contains(searchParams.Keyword.ToLower())) ||
                        (!String.IsNullOrEmpty(x.ClientAccount.Name) && x.ClientAccount.Name.ToLower().Contains(searchParams.Keyword.ToLower())) ||
                        (!String.IsNullOrEmpty(x.CreatedBy) && x.CreatedBy.ToLower().Contains(searchParams.Keyword.ToLower())) ||
                        ((x.ServiceCategory != null) && x.ServiceCategory.Name.ToLower().Contains(searchParams.Keyword.ToLower())) ||
                        (!String.IsNullOrEmpty(x.Notes) && x.Notes.ToLower().Contains(searchParams.Keyword.ToLower())));
                if (!string.IsNullOrEmpty(searchParams?.AuthorityFilter))
                {
                    //because EA is still a field on ClientAccount model, but we might change that.
                    query = query.Where(b => !String.IsNullOrEmpty(b.ClientAccount.ExpenseAuthorityName)
                        && b.ClientAccount.ExpenseAuthorityName.ToLower().Contains(searchParams.AuthorityFilter.ToLower()));
                }
                if (searchParams?.ClientNumber > 0)
                {
                    query = query.Where(x => x.ClientAccountId == searchParams.ClientNumber);
                }
                if (!String.IsNullOrEmpty(searchParams?.Contact))
                {
                    query = query.Where(b => b.ClientAccount.Contacts.Any(c =>
                        c.Person != null &&
                        c.Person.Name.ToLower().Contains(searchParams.Contact.ToLower())) ||
                        (!String.IsNullOrEmpty(b.ClientAccount.ExpenseAuthorityName) && b.ClientAccount.ExpenseAuthorityName.ToLower().Contains(searchParams.Contact.ToLower())));
                }

                query = query.OrderBy(c => c.ClientAccount.Id).ThenBy(c => c.Title).Include(c => c.MostRecentActiveFiscalPeriod);

                IEnumerable<Bill> result = query.AsNoTracking().ToList<Bill>();
                if (restrictToUserContact)
                { //user is a ministry client, and we should only show charges related to accounts they are a contact on
                    result = FilterChargesForCurrentMinistryUser(ministryUserName, result);
                }

                //if we're looking at a previous quarter's charges, make the amounts and quantities reflect what it was for that quarter
                if (previousQuarterChargeIds.Any())
                {
                    foreach (Bill bill in result)
                    {
                        FiscalHistory? chargeHistory = previousQuarterChargeIds.FirstOrDefault(x => x.BillId == bill.Id);
                        if (chargeHistory != null)
                        {
                            bill.Amount = chargeHistory.UnitPriceAtFiscal * chargeHistory.QuantityAtFiscal;
                            bill.Quantity = chargeHistory.QuantityAtFiscal;
                        }
                    }
                }

                //if we're looking at next quarter's charges, calculate the correct quantities based on end dates
                if (isNextQuarter)
                {
                    foreach (Bill bill in result)
                    {
                        decimal calculatedQuantity = _billRepository.CalculateQuantityForQuarter(bill, nextQuarterStart, nextQuarterEnd);
                        bill.Quantity = calculatedQuantity;

                        // Recalculate amount based on the new quantity
                        if (bill.ServiceCategory != null && !String.IsNullOrEmpty(bill.ServiceCategory.Costs))
                        {
                            if (decimal.TryParse(bill.ServiceCategory.Costs, out decimal unitPrice))
                            {
                                bill.Amount = unitPrice * calculatedQuantity;
                            }
                        }
                    }
                }

                return result;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Enumerable.Empty<Bill>();
            }
        }

        private DateTime GetUserLocalTime()
        {
            DateTime utcDate = DateTime.UtcNow;
            TimeZoneInfo pacificZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"); // Handles both PST and PDT
            DateTime pacificTime = TimeZoneInfo.ConvertTimeFromUtc(utcDate, pacificZone);

            return pacificTime;
        }

        private bool IsAccountContact(string ministryUserName, ClientAccount account)
        {/*carmichael, alexander: CITZ"*/
            string[] nameElements = ministryUserName.Split(',');
            string surname = nameElements[0].ToLower();
            nameElements[1] = nameElements[1].TrimStart();
            string firstName = nameElements[1].Substring(0, nameElements[1].IndexOf(" ")).ToLower();

            {
                foreach (Models.Contact contact in account.Contacts)
                {
                    if (contact.Person.DisplayName == ministryUserName)
                        return true;
                    if ((!String.IsNullOrEmpty(account.ExpenseAuthorityName) && (account.ExpenseAuthorityName.ToLower().Contains(surname) && account.ExpenseAuthorityName.ToLower().Contains(firstName)) ||
                        !String.IsNullOrEmpty(account.Approver) && (account.Approver.ToLower().Contains(surname) && account.Approver.ToLower().Contains(firstName)) ||
                        !String.IsNullOrEmpty(account.FinancialContact) && (account.FinancialContact.ToLower().Contains(surname) && account.FinancialContact.ToLower().Contains(firstName)) ||
                        !String.IsNullOrEmpty(account.PrimaryContact) && (account.PrimaryContact.ToLower().Contains(surname) && account.PrimaryContact.ToLower().Contains(firstName))))
                        return true;
                }
            }

            return false;
        }
        private IEnumerable<Bill> FilterChargesForCurrentMinistryUser(string ministryUserName, IEnumerable<Bill> bills)
        {
            try
            {
                return bills.Where(b => (IsAccountContact(ministryUserName, b.ClientAccount)));
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while trying to filter charges based on ministry user contacts");
                _logger.LogError(ex.Message);
                return null;
            }
        }

        [HttpGet]
        public IActionResult WriteToExcel(ChargeIndexSearchParamsModel? searchParams)
        {
            IEnumerable<Bill> bills = QueryForCharges(searchParams);
            try
            {
                string fileName = GetFilename(searchParams, "xlsx");
                using var wb = new XLWorkbook();
                var ws = wb.AddWorksheet();
                var dataTable = new DataTable();
                List<ChargeRow> rows = new List<ChargeRow>();
                // Inserts the collection to Excel as a table with a header row.

                foreach (Bill bill in bills)
                {
                    ServiceCategory? serviceCategory = _categoryRepository.GetById(bill.ServiceCategoryId);
                    ClientAccount? account = _clientAccountRepository.GetClientAccount(bill.ClientAccountId);

                    ChargeRow row = new ChargeRow();
                    row.ChargeId = bill.Id;
                    row.ClientNumber = bill.ClientAccountId;
                    row.ClientName = bill.ClientAccount.Name;
                    row.Program = bill.Title;
                    if (serviceCategory != null)
                    {
                        row.GDXBusArea = serviceCategory?.BusArea?.Name;
                        row.ServiceCategory = serviceCategory?.Name;
                    }
                    row.TicketNumber = bill.TicketNumberAndRequester;
                    row.Amount = bill.Amount;
                    row.Quantity = bill.Quantity;
                    decimal unitPrice;
                    row.UnitPrice = decimal.TryParse(serviceCategory.Costs, out unitPrice) ? unitPrice : null;
                    if (bill.DateCreated != null)
                        row.Created = bill.DateCreated?.DateTime.ToShortDateString();
                    if (bill.StartDate != null)
                        row.Start = bill.StartDate?.DateTime.ToShortDateString();
                    if (bill.EndDate != null)
                        row.End = bill.EndDate?.DateTime.ToShortDateString();

                    row.CreatedBy = bill.CreatedBy;
                    row.AggregateGLCode = bill.ClientAccount.AggregatedGLCode;
                    row.FiscalPeriod = bill.MostRecentActiveFiscalPeriod?.Period;
                    row.IdirOrURL = bill.IdirOrUrl;
                    if (account != null)
                    {
                        if (!String.IsNullOrEmpty(account.ExpenseAuthorityName))
                            row.ExpenseAuthority = account.ExpenseAuthorityName;

                        var primaryContact = account.Contacts?
                                .FirstOrDefault(c => c.ContactType == "primary" && c.Person != null)?
                                    .Person?.DisplayName;
                        row.PrimaryContact = !string.IsNullOrEmpty(primaryContact) ? primaryContact : string.Empty;
                    }
                    row.Notes = bill.Notes;
                    rows.Add(row);

                    if (!string.IsNullOrEmpty(searchParams?.QuarterFilter) && searchParams?.QuarterFilter == "all")
                    {
                        foreach (FiscalHistory fiscalHistory in bill.PreviousFiscalRecords.OrderByDescending(x => x.Id))
                        {
                            if (bill.CurrentFiscalPeriodId == fiscalHistory.FiscalPeriod.Id)
                                continue;
                            row = new ChargeRow();
                            row.ChargeId = bill.Id;
                            row.ClientNumber = bill.ClientAccountId;
                            row.ClientName = bill.ClientAccount.Name;
                            row.Program = bill.Title;
                            if (serviceCategory != null)
                            {
                                row.GDXBusArea = serviceCategory?.BusArea?.Name;
                                row.ServiceCategory = serviceCategory?.Name;
                            }
                            row.TicketNumber = bill.TicketNumberAndRequester;
                            row.Amount = (fiscalHistory.QuantityAtFiscal * fiscalHistory.UnitPriceAtFiscal);
                            row.Quantity = fiscalHistory.QuantityAtFiscal;

                            row.UnitPrice = decimal.TryParse(serviceCategory.Costs, out unitPrice) ? unitPrice : null;
                            if (bill.DateCreated != null)
                                row.Created = bill.DateCreated?.DateTime.ToShortDateString();
                            if (bill.StartDate != null)
                                row.Start = bill.StartDate?.DateTime.ToShortDateString();
                            if (bill.EndDate != null)
                                row.End = bill.EndDate?.DateTime.ToShortDateString();

                            row.CreatedBy = bill.CreatedBy;
                            row.AggregateGLCode = bill.ClientAccount.AggregatedGLCode;
                            row.FiscalPeriod = fiscalHistory.FiscalPeriod?.Period;
                            row.IdirOrURL = bill.IdirOrUrl;
                            if (account != null)
                            {
                                if (!String.IsNullOrEmpty(account.ExpenseAuthorityName))
                                    row.ExpenseAuthority = account.ExpenseAuthorityName;


                                var primaryContact = account.Contacts?
                                    .FirstOrDefault(c => c.ContactType == "primary" && c.Person != null)?
                                        .Person?.DisplayName;
                                row.PrimaryContact = !string.IsNullOrEmpty(primaryContact) ? primaryContact : string.Empty;
                            }
                            row.Notes = bill.Notes;
                            rows.Add(row);
                        }

                    }
                }
                ws.Cell("A1").InsertTable(rows);
                // Adjust column size to contents.
                ws.Column("A").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Column("B").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Columns().AdjustToContents();
                ws.Column("H").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right); //amount
                ws.Column("H").Style.NumberFormat.SetFormat("$##0");

                //ws.Column("H").Style.NumberFormat.SetNumberFormatId(43); // "Accounting" 
                //cell.Style.NumberFormat.SetNumberFormatId(43);
                //ws.Column("H").Cells().DataType = XLDataType.Number;
                ws.Column("J").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center); //quantity
                ws.Column("K").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right); //unit price
                ws.Column("K").Style.NumberFormat.SetFormat("$##0");

                IXLTables tsTables = ws.Tables;
                IXLTable firstTable = tsTables.FirstOrDefault();
                if (firstTable == null)
                {
                    throw new Exception("No table was found for this worksheet.");
                }
                firstTable.Field("ChargeId").Name = "Charge ID";
                firstTable.Field("ClientNumber").Name = "Client Number";
                firstTable.Field("ClientName").Name = "Client Name";
                firstTable.Field("IdirOrURL").Name = "IDIR or URL";
                firstTable.Field("GDXBusArea").Name = "Business Area";
                firstTable.Field("ServiceCategory").Name = "Service Category";
                firstTable.Field("FiscalPeriod").Name = "Fiscal Period";
                firstTable.Field("UnitPrice").Name = "Unit Price";
                firstTable.Field("TicketNumber").Name = "Ticket Number";
                firstTable.Field("AggregateGLCode").Name = "Aggregate GL Code";
                firstTable.Field("CreatedBy").Name = "Created By";
                firstTable.Field("ExpenseAuthority").Name = "ExpenseAuthority";
                firstTable.Field("PrimaryContact").Name = "Primary Contact";

                using var stream = new MemoryStream();
                wb.SaveAs(stream);
                var content = stream.ToArray();
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(content, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not export Excel file. See exception logged below.");
                _logger.LogError($"{ex.Message}");
                return StatusCode(500);
            }
        }

        public IActionResult ShowReport(ChargeIndexSearchParamsModel? searchParams = null)
        {
            IEnumerable<Bill> bills = QueryForCharges(searchParams);
            bills = bills.Where(b => b.ServiceCategoryId != 38 && b.ServiceCategoryId != 69);
            try
            {
                GeneratedReportViewModel model = new GeneratedReportViewModel();
                model.BillingQuarter = !String.IsNullOrEmpty(searchParams?.QuarterFilter) ? searchParams.QuarterFilter : string.Empty;
                Ministry? ministry = null;
                if (searchParams?.MinistryFilter > 0)
                {
                    ministry = _ministryRepository.GetById(searchParams.MinistryFilter);
                }
                model.Ministry = (ministry != null && !String.IsNullOrEmpty(ministry.Title)) ? ministry.Title : string.Empty;
                model.Title = !String.IsNullOrEmpty(searchParams?.TitleFilter) ? searchParams.TitleFilter : string.Empty;
                model.Authority = !String.IsNullOrEmpty(searchParams?.AuthorityFilter) ? searchParams.AuthorityFilter : string.Empty; ;
                model.ClientNumber = searchParams?.ClientNumber > 0 ? (int)searchParams.ClientNumber : -1;

                SortedDictionary<string, decimal> servicesAndSums = GetServicesAndSums(bills, searchParams?.QuarterFilter == "all");


                model.ServicesAndSums = servicesAndSums;


                return View("Report", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }

            return Ok(500);
        }

        [HttpPost]
        public IActionResult ReportToExcel(GeneratedReportViewModel model)
        {
            List<RecordEntry> records = new List<RecordEntry>();
            decimal? total = 0;

            foreach (var entry in model.ServicesAndSums)
            {
                records.Add(new RecordEntry(entry.Key, entry.Value, null));
                total += entry.Value;
            }

            var summedTotal = new List<object>
            {
            new { Id = "Grand Total", Name = "total" },
            };
            string fileName = "QuarterlySummary.xlsx";


            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            List<ChargeRow> rows = new List<ChargeRow>();
            ws.Cell("A1").InsertTable(records);
            // Adjust column size to contents.
            ws.Column("B").Style.NumberFormat.SetFormat("$##0");
            ws.Cell("D1").Value = "Grand Total";
            ws.Cell($"D{model.ServicesAndSums.Count() + 2}").Value = total;
            ws.Column("D").Style.NumberFormat.SetFormat("$##0");
            ws.Column(3).Delete(); // don't need UOM
            ws.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            var content = stream.ToArray();
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(content, contentType, fileName);
        }

        [HttpPost]
        public async Task<IActionResult> PromoteChargesToNewQuarter()
        {
            await _billRepository.PromoteChargesToNewQuarter();

            return Ok(200);
        }
        [HttpGet]
        public IActionResult PromoteCharges()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SetIsActiveForCharge(int id, bool active)
        {
            Bill? charge = _billRepository.GetBill(id);
            if (charge != null)
            {
                charge.IsActive = active;
                await _billRepository.Update(charge);
            }
            else
            {
                _logger.LogError($"Admin user tried to deactivate bill with id {id}, but it was not found in database");
                return BadRequest();
            }
            return Ok(200);
        }

        private SortedDictionary<string, decimal> GetServicesAndSums(IEnumerable<Bill> bills, bool all = false)
        {
            SortedDictionary<string, decimal> servicesAndSums = new SortedDictionary<string, decimal>();

            foreach (Bill bill in bills)
            {
                ServiceCategory? serviceCategory = _categoryRepository.GetById(bill.ServiceCategoryId);
                if (serviceCategory != null)
                {
                    if (servicesAndSums.ContainsKey(serviceCategory.Name))
                    {
                        servicesAndSums[serviceCategory.Name] += (bill.Amount != null) ? bill.Amount.Value : 0;
                        if (all)
                        {
                            foreach (FiscalHistory fiscalHistory in bill.PreviousFiscalRecords.OrderByDescending(x => x.Id))
                            {
                                if (bill.CurrentFiscalPeriodId == fiscalHistory.FiscalPeriod.Id)
                                    continue;
                                decimal? amount = fiscalHistory.UnitPriceAtFiscal * fiscalHistory.QuantityAtFiscal;
                                servicesAndSums[serviceCategory.Name] += (amount != null) ? amount.Value : 0;

                            }
                        }
                    }
                    else
                    {
                        string serviceName = !String.IsNullOrEmpty(serviceCategory.Name) ? serviceCategory.Name
                            : $"Nameless category with ID: {serviceCategory.ServiceId} ";
                        decimal amount = (bill.Amount != null) ? bill.Amount.Value : 0;
                        servicesAndSums.Add(serviceName, amount);
                        if (all)
                            foreach (FiscalHistory fiscalHistory in bill.PreviousFiscalRecords.OrderByDescending(x => x.Id))
                            {
                                if (bill.CurrentFiscalPeriodId == fiscalHistory.FiscalPeriod.Id)
                                    continue;

                                amount = (fiscalHistory.UnitPriceAtFiscal != null && fiscalHistory.QuantityAtFiscal != null) ?
                                    fiscalHistory.UnitPriceAtFiscal.Value * fiscalHistory.QuantityAtFiscal.Value : 0;
                                servicesAndSums[serviceName] += amount;
                            }
                    }
                }
            }

            return servicesAndSums;
        }

        public List<int> GetUserOwnedServiceIds()
        {
            try
            {
                string? userName = User.GetDisplayName(); //Firstname.Lastname@Gov.bc.ca
                if (!string.IsNullOrEmpty(userName))
                {
                    List<int> serviceIds = new List<int>();
                    string[] nameElements = userName.Split('.');
                    if (nameElements.Length > 1)
                    {
                        nameElements[1] = nameElements[1].Substring(0, nameElements[1].IndexOf('@'));
                    }
                    List<int> serviceCategories = _categoryRepository.GetAll()
                        .Where(c => !string.IsNullOrEmpty(c.ServiceOwner)
                        && c.ServiceOwner.ToLower().Contains(nameElements[1].ToLower())).Select(c => c.ServiceId).ToList();
                    return serviceCategories;
                }
                else
                {
                    throw new Exception("No service owner name could be determined based on User info.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return new List<int>();
        }

        private string GetFilename(ChargeIndexSearchParamsModel? searchParams, string extension = "csv")
        {
            string fileName = "Charges";

            Ministry? ministry = null;
            if (searchParams?.MinistryFilter > 0)
            {
                ministry = _ministryRepository.GetById(searchParams.MinistryFilter);
            }
            if (ministry != null && !String.IsNullOrEmpty(ministry.Title))
                fileName += $"-{searchParams?.MinistryFilter}";
            if (!String.IsNullOrEmpty(searchParams?.TitleFilter))
                fileName += $"-{searchParams.TitleFilter}";

            if (!String.IsNullOrEmpty(searchParams?.AuthorityFilter))
                fileName += $"-{searchParams.AuthorityFilter}";

            fileName += DateTime.Today.ToString("dd-mm-yyyy");
            fileName += $".{extension}";

            return fileName;
        }
    }

    // For exporting (filtered) charges from the Index view
    public class ChargeRow
    {
        public int ChargeId { get; set; }
        public int ClientNumber { get; set; }
        public string? ClientName { get; set; }
        public string? Program { get; set; } //"Title"
        public string? IdirOrURL { get; set; }
        public string? GDXBusArea { get; set; }
        public string? ServiceCategory { get; set; }
        public Decimal? Amount { get; set; }
        public string? FiscalPeriod { get; set; }
        public Decimal? Quantity { get; set; }
        public Decimal? UnitPrice { get; set; }
        public string? TicketNumber { get; set; }
        public string? Created { get; set; }
        public string? Start { get; set; }
        public string? End { get; set; }
        public string? AggregateGLCode { get; set; }
        public string? CreatedBy { get; set; }
        public string? ExpenseAuthority { get; set; }
        public string? PrimaryContact { get; set; }
        public string? Notes { get; set; }
    }

    // For creating the exported quarterly reports.
    public class RecordEntry
    {
        public string ServiceCategory { get; set; }
        public decimal? Amount { get; set; }
        public string? UOM { get; set; }
        public decimal? Quantity { get; set; }

        public RecordEntry(string serviceCategory, decimal? amount, decimal? quantity)
        {
            ServiceCategory = serviceCategory;
            Amount = amount;
            Quantity = quantity;
        }
    }
}
