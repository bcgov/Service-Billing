using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
                default:
                    ViewData["FiscalPeriod"] = _billRepository.DetermineCurrentQuarter();
                    break;
                case "previous":
                    ViewData["FiscalPeriod"] = _billRepository.GetPreviousQuarterString();
                    break;
                case "next":
                    ViewData["FiscalPeriod"] = _billRepository.DetermineCurrentQuarter(_billRepository.DetermineStartOfNextQuarter());
                    break;
                case "all":
                    ViewData["FiscalPeriod"] = "all";
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

            IEnumerable<Bill> bills = QueryForCharges(searchModel, !String.IsNullOrEmpty(ministryUserName) ? ministryUserName : String.Empty);
            int count = bills.Any() ? bills.Count() : 0;
            
            if(searchModel != null && !String.IsNullOrEmpty(searchModel.QuarterFilter))
            {
                switch (searchModel.QuarterFilter)
                {
                    case "current": default:
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
                        foreach(Bill bill in bills)
                        {
                            count++;
                            if (bill.PreviousFiscalRecords.Any())
                                count += bill.PreviousFiscalRecords.Where(x => x.FiscalPeriod.Id != bill.CurrentFiscalPeriodId).Count();
                        }
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
                if(historyId > 0)
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
            if(historyId != null)
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
            }

            return RedirectToAction(nameof(Index));
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
                ClientAccount? account = _clientAccountRepository.GetClientAccount(bill.ClientAccountId);
                ServiceCategory? category = _categoryRepository.GetById(bill.ServiceCategoryId);
                if (account == null || category == null)
                {
                    throw new Exception("Could not find either a client account or service category when attempting to create new charge entry.");
                }
                if(string.IsNullOrEmpty(bill.CreatedBy))
                    bill.CreatedBy = await GetMyName();
              
                bill.ClientAccount = account;
                bill.ServiceCategory = category;
                DetermineCurrentQuarter(bill, bill.StartDate); // Note: StartDate could be earlier than current quarter
                FiscalPeriod? fiscalPeriod = _fiscalPeriodRepository.GetFiscalPeriodById(bill.CurrentFiscalPeriodId);
                if (fiscalPeriod == null)
                    throw new Exception($"A fiscal period with id: {bill.CurrentFiscalPeriodId} could not be found");
                bill.MostRecentActiveFiscalPeriod = fiscalPeriod;
                _logger.LogInformation($"New charge is valid");

                int billId = await _billRepository.CreateBill(bill);
                bill = _billRepository.GetBill(billId);

                // Has a StartDate Earlier than the start of this Quarter been selected?
                if(bill?.StartDate != null && bill.StartDate.Value < _billRepository.DetermineStartOfCurrentQuarter()) 
                {
                    await _billRepository.PromoteCharge(
                        bill, 
                        _fiscalPeriodRepository.GetFiscalPeriodByString(_billRepository.DetermineCurrentQuarter())
                    );
                }

                return RedirectToAction($"Details", new { id = bill?.Id, historyId = string.Empty, isNew = true });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Creating a new charge failed to write to database. Exception: {ex.InnerException}");
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View();
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
                /* if UOM is month, then we should adjust the quantity such that the client is not charged for any months
                 * past the start of the quarter. When such a bill is advanced to the future fiscal period, its quantity
                 * will typically be set to three, unless it has an end date sooner than the end of that quarter
                 * */
                if(category?.UOM?.ToLower() == "month" && !quantityChanged)
                {
                    DateTimeOffset start = new DateTimeOffset();
                    DateTimeOffset end = new DateTimeOffset();
                    DateTimeOffset quarterStart = _billRepository.DetermineStartOfCurrentQuarter();
                    DateTimeOffset quarterEnd = _billRepository.DetermineEndOfQuarter(quarterStart.Date);
                    int startMonthDifference = 0;
                    int endMonthDifference = 0;
                    if (!String.IsNullOrEmpty(startDate))
                    {
                        if (DateTimeOffset.TryParse(startDate, out start) && start > quarterStart)
                        {
                            startMonthDifference = start.Month - quarterStart.Month;
                        }
                    }
                    if (!String.IsNullOrEmpty(endDate))
                    {
                        if (DateTimeOffset.TryParse(endDate, out end) && end < quarterEnd)
                        {
                            int monthScalar = (int)(end.Year - quarterEnd.Year) > 0 ? (int)(end.Year - quarterEnd.Year) : 1;
                            endMonthDifference = Math.Min(monthScalar * quarterEnd.Month - end.Month, 3);
                        }
                    }
                    quantity = Math.Max(3 - (startMonthDifference + endMonthDifference), 0);
                }
                decimal newAmount;
                string cost = !String.IsNullOrEmpty(category?.Costs)? category.Costs : "0";
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
               
                RecordEntry recordEntry = new RecordEntry(!String.IsNullOrEmpty(category?.Name)? category.Name : "NoCategoryName", newAmount * quantity, quantity);
                recordEntry.UOM = UOM;

                return new JsonResult(recordEntry);
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
                    if(CurrentFiscalPeriod == null )
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
                bill.CurrentFiscalPeriodId= CurrentFiscal.Id;
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
                    .Include(b => b.ClientAccount)
                    .Include(b => b.ServiceCategory)
                    .Include(b => b.PreviousFiscalRecords)
                    .ThenInclude(r => r.FiscalPeriod);

               //Todo: Give this some more thought. Can probably simplify this logic.
                bool restrictToOwnedServices = (!User.IsInRole("GDXBillingService.FinancialOfficer")
                    && User.IsInRole("GDXBillingService.Owner"));
                bool restrictToUserContact = (!User.IsInRole("GDXBillingService.FinancialOfficer")
                    && !User.IsInRole("GDXBillingService.Owner"));

                List<FiscalHistory> previousQuarterChargeIds = new List<FiscalHistory>();
                switch (searchParams?.QuarterFilter)
                {
                    case "current":
                    default:
                        string fiscalPeriodString = _billRepository.DetermineCurrentQuarter();
                        FiscalPeriod? fiscalPeriod = _fiscalPeriodRepository.GetFiscalPeriodByString(fiscalPeriodString);
                        if(fiscalPeriod == null)
                        {
                            throw new Exception($"No Fiscal Period entity was found that matches \"{fiscalPeriodString}\"");
                        }
                        query = query.Where(b => b.CurrentFiscalPeriodId == fiscalPeriod.Id);
                        query = query.Where(b => b.IsActive);
                        query = query.Where(b => b.ClientAccount.IsActive);
                        break;
                    case "previous":
                        previousQuarterChargeIds = _billRepository.GetPreviousQuarterChargeHistory().ToList();
                        query = query.Where(b => previousQuarterChargeIds.Select(x => x.BillId).Contains(b.Id));
                        break;
                    case "next":
                        List<int> idsOfFixedServices = _billRepository.GetFixedServices();
                        DateTime startOfNextQuarter = _billRepository.DetermineStartOfNextQuarter();
                        query = query.Where(b => idsOfFixedServices.Contains(b.ServiceCategoryId) && (b.EndDate == null || b.EndDate > startOfNextQuarter));
                        query = query.Where(b => b.IsActive);
                        query = query.Where(b => b.ClientAccount.IsActive);
                        ViewData["FiscalPeriod"] = _billRepository.DetermineCurrentQuarter(_billRepository.DetermineStartOfNextQuarter());
                        break;
                    case "all":
                        // Just break. Effectively it's just one less Where clause
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
                    query = query.Where(b => !String.IsNullOrEmpty(b.ClientAccount.ExpenseAuthorityName) && b.ClientAccount.ExpenseAuthorityName.ToLower().Contains(searchParams.AuthorityFilter.ToLower()));
                }
                if (searchParams?.ClientNumber > 0)
                {
                    query = query.Where(x => x.ClientAccountId == searchParams.ClientNumber);
                }
                if (!String.IsNullOrEmpty(searchParams?.PrimaryContact)) //Note: not if current user is primary contact, rather searching for charges by an arbitrary primary contat.
                {
                    query = query.Where(b => b.ClientAccount.PrimaryContact != null && b.ClientAccount.PrimaryContact.ToLower().Contains(searchParams.PrimaryContact.ToLower()));
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
                        if(chargeHistory != null)
                        {
                            bill.Amount = chargeHistory.UnitPriceAtFiscal * chargeHistory.QuantityAtFiscal;
                            bill.Quantity = chargeHistory.QuantityAtFiscal;
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

            return (!String.IsNullOrEmpty(account.ExpenseAuthorityName) && (account.ExpenseAuthorityName.ToLower().Contains(surname) && account.ExpenseAuthorityName.ToLower().Contains(firstName)) ||
                !String.IsNullOrEmpty(account.Approver) && (account.Approver.ToLower().Contains(surname) && account.Approver.ToLower().Contains(firstName)) ||
                !String.IsNullOrEmpty(account.FinancialContact) && (account.FinancialContact.ToLower().Contains(surname) && account.FinancialContact.ToLower().Contains(firstName)) ||
                !String.IsNullOrEmpty(account.PrimaryContact) && (account.PrimaryContact.ToLower().Contains(surname) && account.PrimaryContact.ToLower().Contains(firstName)));
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

                    if (!string.IsNullOrEmpty(searchParams?.QuarterFilter) && searchParams?.QuarterFilter == "all")
                    {
                        foreach (FiscalHistory fiscalHistory in bill.PreviousFiscalRecords.OrderByDescending(x => x.Id))
                        {
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
                            row.Amount = String.Format("${0:.##}", (fiscalHistory.QuantityAtFiscal * fiscalHistory.UnitPriceAtFiscal));
                            row.Quantity = fiscalHistory.QuantityAtFiscal;
                            row.UnitPrice = String.Format("${0:.##}", fiscalHistory.UnitPriceAtFiscal.ToString());
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
                                if (!String.IsNullOrEmpty(account.PrimaryContact))
                                    row.PrimaryContact = account.PrimaryContact;
                            }
                            row.Notes = bill.Notes;
                            rows.Add(row);
                        }
                    }
                    else
                    {
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
                        row.Amount = @String.Format("${0:.##}", bill.Amount);
                        row.Quantity = bill.Quantity;
                        row.UnitPrice = !String.IsNullOrEmpty(serviceCategory?.Costs) ? @String.Format("${0:.##}", serviceCategory.Costs) : "";
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
                            if (!String.IsNullOrEmpty(account.PrimaryContact))
                                row.PrimaryContact = account.PrimaryContact;
                        }
                        row.Notes = bill.Notes;
                        rows.Add(row);
                    }
                } 
                ws.Cell("A1").InsertTable(rows);
                // Adjust column size to contents.
                ws.Column("A").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Column("B").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Columns().AdjustToContents();
                ws.Column("H").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right); //amount
                ws.Column("J").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center); //quantity
                ws.Column("K").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right); //unit price


                IXLTables tsTables = ws.Tables;
                IXLTable firstTable = tsTables.FirstOrDefault();
                if(firstTable == null) 
                {
                    throw new Exception("No table was found for this worksheet.");
                }
                firstTable.Field("ChargeId").Name = "Charge ID";
                firstTable.Field("ClientNumber").Name = "Client Number";
                firstTable.Field("ClientName").Name = "Client Name";
                firstTable.Field("IdirOrURL").Name = "IDIR or URL";
                firstTable.Field("GDXBusArea").Name = "GDX Business Area";
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
               
                SortedDictionary<string, decimal?> servicesAndSums = GetServicesAndSums(bills);


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
            ws.Cell("D1").Value = "Grand Total";
            ws.Cell($"D{model.ServicesAndSums.Count() + 2}").Value = total;
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

        private SortedDictionary<string, decimal?> GetServicesAndSums(IEnumerable<Bill> bills)
        {
            SortedDictionary<string, decimal?> servicesAndSums = new SortedDictionary<string, decimal?>();

            foreach (Bill bill in bills)
            {
                ServiceCategory? serviceCategory = _categoryRepository.GetById(bill.ServiceCategoryId);
                if (serviceCategory != null)
                {
                    if (servicesAndSums.ContainsKey(serviceCategory.Name))
                    {
                        servicesAndSums[serviceCategory.Name] += bill.Amount;
                    }
                    else
                    {
                        servicesAndSums.Add(!String.IsNullOrEmpty(serviceCategory.Name) ? serviceCategory.Name
                            : $"Nameless category with ID: {serviceCategory.ServiceId} ", bill.Amount);
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
            //if (!string.IsNullOrEmpty(searchParams?.QuarterFilter))
            //{
            //    if (searchParams.QuarterFilter == "all")
            //        fileName += "-all-Quarters";
            //    if (bills.Any())
            //    {
            //        fileName += $"={bills.First().BillingCycle}";
            //    }
            //}
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
        public string? Amount { get; set; }
        public string? FiscalPeriod { get; set; }
        public Decimal? Quantity { get; set; }
        public string? UnitPrice { get; set; }
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
