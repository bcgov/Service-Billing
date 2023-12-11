using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Microsoft.Graph.Search;
using Microsoft.Identity.Web;
using MimeKit;
using Service_Billing.Extensions;
using Service_Billing.Models;
using Service_Billing.Models.Repositories;
using Service_Billing.ViewModels;
using System.Collections.Immutable;
using System.Security.Claims;

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

        public BillsController(ILogger<BillsController> logger,
            IBillRepository billRepository,
            IServiceCategoryRepository categoryRepository,
            IClientAccountRepository clientAccountRepository,
            IMinistryRepository ministryRepository,
            IAuthorizationService authorizationService,
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

        }

        [Authorize]
        [Authorize(Roles = "GDXBillingService.FinancialOfficer, GDXBillingService.Owner, GDXBillingService.User")]
        public IActionResult Index(ChargeIndexSearchParamsModel searchModel)
        {
            IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();
            IEnumerable<ClientAccount> clients = _clientAccountRepository.GetAll();
            IEnumerable<Ministry> ministries = _ministryRepository.GetAll();
            if (ministries != null && ministries.Any())
            {
                ViewData["Ministries"] = ministries;
            }
            if (categories != null && categories.Any())
            {
                ViewBag.ServiceCategories = categories.ToList();
            }
            // restrict items based on user's privileges
            searchModel.ShouldRestrictToUserOwnedServices = (!User.IsInRole("GDXBillingService.FinancialOfficer")
                && User.IsInRole("GDXBillingService.Owner"));

            ViewData["searchModel"] = searchModel;

            IEnumerable<Bill> bills = GetFilteredBills(searchModel);
            /* filter out categories we don't bill on. Hardcoding this is probably not the best bet. We should come up with a better scheme */
            bills = bills.Where(b => b.ServiceCategoryId != 38 && b.ServiceCategoryId != 69);
            var authUser = User;
            if (authUser.IsMinistryClient(_authorizationService))
            {
                var name = authUser?.FindFirst("name");
                if (name is not null) ViewData["NameClaim"] = name.Value;
            }

            return View(new AllBillsViewModel(bills, categories, clients));
        }

        public ActionResult Details(int id)
        {
            Bill? bill = _billRepository.GetBill(id);
            if (bill == null)
            {
                return NotFound();
            }
            ClientAccount? account = _clientAccountRepository.GetClientAccount(bill.ClientAccountId);
            ServiceCategory? serviceCategory = _categoryRepository.GetById(bill.ServiceCategoryId);
            ViewData["clientAccount"] = account != null ? account : "";
            ViewData["serviceCategory"] = serviceCategory != null ? serviceCategory : "";
            return View(bill);
        }

        public ActionResult Edit(int id)
        {
            Bill? bill = _billRepository.GetBill(id);
            _logger.LogInformation($"Editing Bill with ID: {id}");
            if (bill == null)
                _logger.LogWarning($"Bill with Id: {id} was not found in database");
            IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();
            if (bill == null)
                return NotFound();

            bill.DateModified = DateTime.Now;
            if (String.IsNullOrEmpty(bill.FiscalPeriod) || String.IsNullOrEmpty(bill.BillingCycle))
                DetermineCurrentQuarter(bill, bill.DateCreated);
            ViewData["Client"] = _clientAccountRepository.GetClientAccount(bill.ClientAccountId);
            ViewData["Categories"] = categories;
            ServiceCategory? serviceCategory = _categoryRepository.GetById(bill.ServiceCategoryId);
            ViewData["serviceCategory"] = serviceCategory != null ? serviceCategory : "";

            return View(bill);
        }

        // POST: ClientAccountController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Bill bill)
        {

            try
            {
                await _billRepository.Update(bill);
                return View("details", bill);
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
            ViewData["CurrentUser"] = await GetMyName();
            Bill bill = new Bill();
            bill.DateCreated = DateTime.Now;
            DetermineCurrentQuarter(bill, bill.DateCreated);
            if (accountId > 0)
            {
                ClientAccount? account = _clientAccountRepository.GetClientAccount(accountId);
                if (account != null)
                {
                    bill.ClientAccountId = accountId;
                    bill.ClientName = account.Name;
                }
            }

            return View(bill);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]  //[Bind(Include = "LastName, FirstMidName, EnrollmentDate")]Student student)
                                    // public async Task<IActionResult> Create(IFormCollection collection)
        public async Task<ActionResult> Create(Bill bill)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _logger.LogInformation($"New charge is valid");
                    // Add aggregate gl code. 
                    string aggregateCode = string.Empty;
                    ClientAccount? account = _clientAccountRepository.GetClientAccount(bill.ClientAccountId);
                    if (account != null)
                    {
                        aggregateCode = $"{account.ClientNumber}.{account.ResponsibilityCentre}." +
                            $"{account.ServiceLine}.{account.STOB}.{account.Project}";
                    }
                    bill.AggregateGLCode = aggregateCode;

                    await _billRepository.CreateBill(bill);
                }
                else
                {
                    _logger.LogWarning($"New charge is not in a valid model state");
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Creating a new charge failed to write to database. Exception: {ex.InnerException}");
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return RedirectToAction(nameof(Details), bill);
        }

        [HttpGet]
        public ActionResult GetBillAmount(short? serviceId, decimal? quantity)
        {
            try
            {
                if (serviceId == null || quantity == null)
                    throw new Exception("Cannot calculate bill amount because categoryId or quantity is null");
                ServiceCategory category = _categoryRepository.GetById(serviceId);
                if (category == null)
                {
                    throw new Exception($"Service category with id: {serviceId} not found!");
                }
                decimal newAmount;
                string cost = category.Costs;
                if (!string.IsNullOrEmpty(cost) && cost.Contains('$'))
                {
                    cost = cost.Replace('$', ' ');
                    cost = cost.Trim();
                }
                if (!decimal.TryParse(cost, out newAmount))
                {
                    newAmount = 0;
                }
                if (category.ServiceId == 5)
                    newAmount = 85;
                string? UOM = !string.IsNullOrEmpty(category.UOM) ? category.UOM : "n/a";
                RecordEntry recordEntry = new RecordEntry(category.Name, newAmount * quantity);
                recordEntry.UOM = UOM;

                return new JsonResult(recordEntry);
            }
            catch (Exception ex)
            {
                //TODO: log exception
                return new JsonResult(ex.Message);
            }
        }

        [HttpGet]
        public ActionResult GetClients()
        {
            try
            {
                IEnumerable<ClientAccount> accounts = _clientAccountRepository.GetAll();
                List<ClientAccount> result = accounts.ToList();
                return new JsonResult(accounts.ToList());
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }
        }

        public void DetermineCurrentQuarter(Bill bill, DateTime? date = null)
        {
            DateTime today = DateTime.Today;
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
                    bill.BillingCycle = new DateTime(today.Year, 4, 1).ToString("yyyy-MM-dd");
                    break;
                case 7:
                case 8:
                case 9:
                    quarter = "Quarter 2";
                    bill.BillingCycle = new DateTime(today.Year, 7, 1).ToString("yyyy-MM-dd");
                    break;
                case 10:
                case 11:
                case 12:
                    quarter = "Quarter 3";
                    bill.BillingCycle = new DateTime(today.Year, 10, 1).ToString("yyyy-MM-dd");
                    break;
                case 1:
                case 2:
                case 3:
                    quarter = "Quarter 4";
                    bill.BillingCycle = new DateTime(today.Year, 1, 1).ToString("yyyy-MM-dd");
                    break;
            }

            bill.FiscalPeriod = $"Fiscal {year1.Substring(2)}/{year2.Substring(2)} {quarter}";
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
                return "Could not get user's Graph name";
            }
        }

        private IEnumerable<Bill> GetFilteredBills(ChargeIndexSearchParamsModel? searchParams)
        {
            try
            {

                IEnumerable<Bill> bills;
                switch (searchParams?.QuarterFilter)
                {
                    case "current":
                    default:
                        bills = _billRepository.GetCurrentQuarterBills();
                        break;
                    case "previous":
                        bills = _billRepository.GetPreviousQuarterBills();
                        break;
                    case "next":
                        bills = _billRepository.GetNextQuarterBills();
                        break;
                    case "all":
                        bills = _billRepository.AllBills;
                        break;
                }
                // now filter the results
                if (searchParams?.Inactive != null && (bool)(searchParams?.Inactive))
                {
                    bills = bills.Where(b => b.IsActive);
                }
                if (searchParams?.ShouldRestrictToUserOwnedServices != null && searchParams.ShouldRestrictToUserOwnedServices)
                {
                    string? userName = User.GetDisplayName(); //Alexander.Carmichael@Gov.bc.ca
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
                        bills = bills.Where(b => serviceCategories.Contains(b.ServiceCategoryId));
                    }
                    else
                    {
                        throw new Exception("No service owner name could be determined based on User info.");
                    }
                }
                if (!string.IsNullOrEmpty(searchParams?.MinistryFilter))
                    bills = bills.Where(x => !String.IsNullOrEmpty(x.ClientName) && x.ClientName.ToLower().Contains(searchParams.MinistryFilter.ToLower()));
                if (!string.IsNullOrEmpty(searchParams?.TitleFilter))
                    bills = bills.Where(x => !String.IsNullOrEmpty(x.Title) && x.Title.ToLower().Contains(searchParams.TitleFilter.ToLower()));
                if (searchParams?.CategoryFilter > 0)
                    bills = bills.Where(x => x.ServiceCategoryId == searchParams?.CategoryFilter);
                if (!string.IsNullOrEmpty(searchParams?.Keyword))
                    bills = bills.Where(x => (!String.IsNullOrEmpty(x.Title) && x.Title.ToLower().Contains(searchParams.Keyword.ToLower())) ||
                       (!String.IsNullOrEmpty(x.IdirOrUrl) && x.IdirOrUrl.ToLower().Contains(searchParams.Keyword.ToLower())) ||
                        (!String.IsNullOrEmpty(x.ClientName) && x.ClientName.ToLower().Contains(searchParams.Keyword.ToLower())) ||
                        (!String.IsNullOrEmpty(x.CreatedBy) && x.CreatedBy.ToLower().Contains(searchParams.Keyword.ToLower())));
                if (!string.IsNullOrEmpty(searchParams?.AuthorityFilter))
                {
                    List<Bill> filteredBills = new List<Bill>();
                    foreach (Bill bill in bills)
                    {
                        ClientAccount? account = _clientAccountRepository.GetClientAccount(bill.ClientAccountId);
                        if (account != null && !String.IsNullOrEmpty(account.ExpenseAuthorityName) && account.ExpenseAuthorityName.Contains(searchParams.AuthorityFilter))
                        {
                            filteredBills.Append(bill);
                        }
                    }

                    bills = filteredBills;

                }
                if (searchParams?.ClientNumber > 0)
                {
                    // int clientId = _clientAccountRepository.GetClientIdFromClientNumber(clientNumber);
                    bills = bills.Where(x => x.ClientAccountId == searchParams.ClientNumber);
                }

                return bills;
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while trying to filter charges in Index view");
                _logger.LogError(ex.Message);
                return null;
            }
        }

        [HttpGet]
        public async Task<IActionResult> WriteToCSV(ChargeIndexSearchParamsModel? searchParams)
        {
            IEnumerable<Bill> bills = GetFilteredBills(searchParams);
            try
            {
                using var memoryStream = new MemoryStream();
                using (var streamWriter = new StreamWriter(memoryStream))
                {
                    using var csvWriter = new CsvWriter(streamWriter);
                    csvWriter.WriteHeader<ChargeRow>();
                    csvWriter.NextRecord();
                    foreach (Bill bill in bills)
                    {
                        ServiceCategory? serviceCategory = _categoryRepository.GetById(bill.ServiceCategoryId);
                        ClientAccount? account = _clientAccountRepository.GetClientAccount(bill.ClientAccountId);
                        ChargeRow row = new ChargeRow();
                        row.ClientNumber = bill.ClientAccountId;
                        row.ClientName = bill.ClientName;
                        row.Program = bill.Title;
                        if (serviceCategory != null)
                        {
                            row.GDXBusArea = serviceCategory.GDXBusArea;
                            row.ServiceCategory = serviceCategory.Name;
                        }
                        row.TicketNumber = bill.TicketNumberAndRequester;
                        row.Amount = @String.Format("${0:.##}", bill.Amount);
                        row.Quantity = bill.Quantity;
                        row.UnitPrice = !String.IsNullOrEmpty(serviceCategory?.Costs) ? @String.Format("${0:.##}", serviceCategory.Costs) : "";
                        row.Created = bill.DateCreated?.Date;
                        row.Start = bill.StartDate;
                        row.End = bill.EndDate;
                        row.CreatedBy = bill.CreatedBy;
                        row.AggregateGLCode = bill.AggregateGLCode;
                        row.FiscalPeriod = bill.FiscalPeriod;
                        row.IdirOrURL = bill.IdirOrUrl;
                        if (account != null && !String.IsNullOrEmpty(account.ExpenseAuthorityName))
                            row.ExpenseAuthority = account.ExpenseAuthorityName;

                        csvWriter.WriteRecord(row);
                        csvWriter.NextRecord();
                    }

                    //  csvWriter.WriteRecords(bills);
                }
                string fileName = "Charges";
                if (!string.IsNullOrEmpty(searchParams?.QuarterFilter))
                {
                    if (searchParams.QuarterFilter == "all")
                        fileName += "-all-Quarters";
                    if (bills.Any())
                    {
                        fileName += $"={bills.First().BillingCycle}";
                    }
                }
                if (!String.IsNullOrEmpty(searchParams?.MinistryFilter))
                    fileName += $"-{searchParams.MinistryFilter}";
                if (!String.IsNullOrEmpty(searchParams?.TitleFilter))
                    fileName += $"-{searchParams.TitleFilter}";
                if (searchParams?.CategoryFilter > 0)
                {
                    ServiceCategory? category = _categoryRepository.GetById(searchParams.CategoryFilter);
                    if (category != null)
                        fileName += $"-{category.Name}";
                }
                if (!String.IsNullOrEmpty(searchParams?.AuthorityFilter))
                    fileName += $"-{searchParams.AuthorityFilter}";

                fileName += DateTime.Today.ToString("dd-mm-yyyy");
                fileName += ".csv";

                return File(memoryStream.ToArray(), "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                //we really need an error page
                return StatusCode(500);
            }
        }

        public async Task<IActionResult> ShowReport(ChargeIndexSearchParamsModel? searchParams = null)
        {
            IEnumerable<Bill> bills = GetFilteredBills(searchParams);
            bills = bills.Where(b => b.ServiceCategoryId != 38 && b.ServiceCategoryId != 69);
            try
            {
                GeneratedReportViewModel model = new GeneratedReportViewModel();
                model.BillingQuarter = !String.IsNullOrEmpty(searchParams?.QuarterFilter) ? searchParams.QuarterFilter : string.Empty;
                model.Ministry = !String.IsNullOrEmpty(searchParams?.MinistryFilter) ? searchParams.MinistryFilter : string.Empty;
                model.Title = !String.IsNullOrEmpty(searchParams?.TitleFilter) ? searchParams.TitleFilter : string.Empty;
                model.Authority = !String.IsNullOrEmpty(searchParams?.AuthorityFilter) ? searchParams.AuthorityFilter : string.Empty; ;
                model.ClientNumber = searchParams?.ClientNumber > 0 ? (int)searchParams.ClientNumber : -1;
                if (searchParams?.CategoryFilter > 0)
                {
                    ServiceCategory? serviceCategory = _categoryRepository.GetById(searchParams.CategoryFilter);
                    if (serviceCategory != null && !String.IsNullOrEmpty(serviceCategory.Name))
                        model.Service = serviceCategory.Name;
                }
                model.ServiceCategoryId = searchParams?.CategoryFilter > 0 ? (int)searchParams.CategoryFilter : -1;
                SortedDictionary<string, decimal?> servicesAndSums = GetServicesAndSums(bills);


                model.ServicesAndSums = servicesAndSums;


                return View("Report", model);
            }
            catch (Exception ex)
            {

            }

            return Ok(500);
        }

        [HttpGet]
        public async Task<IActionResult> ReportToCSV(ChargeIndexSearchParamsModel? searchParams)
        {
            IEnumerable<Bill> bills = GetFilteredBills(searchParams);
            SortedDictionary<string, decimal?> servicesAndSums = GetServicesAndSums(bills);
            List<RecordEntry> records = new List<RecordEntry>();
            decimal? total = 0;

            foreach (var entry in servicesAndSums)
            {
                records.Add(new RecordEntry(entry.Key, entry.Value));
                total += entry.Value;
            }

            var summedTotal = new List<object>
{
new { Id = "Grand Total", Name = total },
};
            using var memoryStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                using var csvWriter = new CsvWriter(streamWriter);
                csvWriter.WriteRecords(records);
                csvWriter.WriteRecords(summedTotal);
            }
            string fileName = "GeneratedReport.csv";

            return File(memoryStream.ToArray(), "application/octet-stream", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> PromoteChargesToNewQuarter()
        {
            await _billRepository.PromoteChargesToNewQuarter();

            return Ok(200);
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
    }

    // For exporting (filtered) charges from the Index view
    public class ChargeRow
    {
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
        public DateTime? Created { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string? AggregateGLCode { get; set; }
        public string? CreatedBy { get; set; }
        public string? ExpenseAuthority { get; set; }
    }


    // For creating the exported quarterly reports. 
    public class RecordEntry
    {
        public string ServiceCategory { get; set; }
        public decimal? Amount { get; set; }
        public string UOM { get; set; }

        public RecordEntry(string serviceCategory, decimal? amount)
        {
            ServiceCategory = serviceCategory;
            Amount = amount;
        }
    }
}
