using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Service_Billing.Models;
using Service_Billing.ViewModels;

namespace Service_Billing.Controllers
{
    public class BillsController : Controller
    {
        private readonly IBillRepositroy _billRepository;
        private readonly IServiceCategoryRepository _categoryRepository;
        private readonly IClientAccountRepository _clientAccountRepository;
        private readonly IMinistryRepository _ministryRepository;
        private readonly GraphServiceClient _graphServiceClient;
        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;

        public BillsController(IBillRepositroy billRepository,
            IServiceCategoryRepository categoryRepository,
            IClientAccountRepository clientAccountRepository,
            IMinistryRepository ministryRepository,
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
        }

        public IActionResult Index(string quarterFilter, 
            string ministryFilter,
            string titleFilter, 
            int categoryFilter,
            string authorityFilter,
            bool meFilter,
            int clientNumber)
        {
            IEnumerable<Bill> bills;
            IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();
            IEnumerable<ClientAccount> clients = _clientAccountRepository.GetAll();
            IEnumerable<Ministry> ministries = _ministryRepository.GetAll();
            ViewData["Ministries"] = ministries;
            ViewBag.ServiceCategories = categories.ToList();
            ViewData["QuarterFilter"] = quarterFilter;
            ViewData["MinistryFilter"] = ministryFilter;
            ViewData["TitleFilter"] = titleFilter;
            ViewData["CategoryFilter"] = categoryFilter;
            ViewData["AuthorityFilter"] = authorityFilter;
            ViewData["ClientNumber"] = clientNumber;
            //ViewData["MeFitler"] = meFilter;

            switch (quarterFilter)
            {
                case "current":
                default:
                    ViewData["Quarter"] = "Current Quarter";
                    bills = _billRepository.GetCurrentQuarterBills();
                    break;
                case "previous":
                    ViewData["Quarter"] = "Previous Quarter";
                    bills = _billRepository.GetPreviousQuarterBills();
                    break;
                case "all":
                    ViewData["Quarter"] = "All Quarters";
                    bills = _billRepository.AllBills;
                    break;
            }
            // now filter the results
            if (!string.IsNullOrEmpty(ministryFilter))
                bills = bills.Where(x => x.ClientName.ToLower().Contains(ministryFilter.ToLower()));
            if (!string.IsNullOrEmpty(titleFilter))
                bills = bills.Where(x => x.Title.ToLower().Contains(titleFilter.ToLower()));
            if (categoryFilter > 0)
                bills = bills.Where(x => x.ServiceCategoryId == categoryFilter);
            if (!string.IsNullOrEmpty(authorityFilter))
            {
                List<Bill> filteredBills = new List<Bill>();
                foreach(Bill bill in bills)
                {
                    ClientAccount? account = _clientAccountRepository.GetClientAccount(bill.ClientAccountId);
                    if(account != null && !String.IsNullOrEmpty(account.ExpenseAuthorityName) && account.ExpenseAuthorityName.Contains(authorityFilter))
                    {
                        filteredBills.Append(bill);
                    }
                }

                bills = filteredBills;
            }
            if(clientNumber > 0)
            {
                int clientId = _clientAccountRepository.GetClientIdFromClientNumber(clientNumber);
                bills = bills.Where(x => x.ClientAccountId == clientId);
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
            IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();//.Where(c => c.isActive == true);
            if (bill == null)
                return NotFound();

            bill.DateModified = DateTime.Now;
            if (String.IsNullOrEmpty(bill.FiscalPeriod) || String.IsNullOrEmpty(bill.BillingCycle))
                DetermineCurrentQuarter(bill, bill.DateCreated);
            ViewData["Client"] = _clientAccountRepository.GetClientAccount(bill.ClientAccountId);
            ViewData["Categories"] = categories;

            return View(bill);
        }

        // POST: ClientAccountController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormCollection collection)
        {
            Bill? billToUpdate = _billRepository.GetBill(id);
            if (billToUpdate == null)
            {
                return NotFound();
            }
            if (await TryUpdateModelAsync<Bill>(billToUpdate, "",
                b => b.ClientAccountId,
                b => b.ClientName,
                b => b.Title,
                b => b.IdirOrUrl,
                b => b.ServiceCategoryId,
                b => b.Amount,
                b => b.Quantity,
                b => b.TicketNumberAndRequester,
                b => b.DateModified,
                b => b.CreatedBy
                ))
            {
                try
                {
                    await _billRepository.CreateBill(billToUpdate);
                }
                catch (DbUpdateException /* ex */)
                {
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }

                return RedirectToAction(nameof(Index));
            }

            return Details(id);
        }

        [HttpGet]
        public ActionResult Create()
        {
            IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();
            ViewData["Categories"] = categories;
            Bill bill = new Bill();
            bill.DateCreated = DateTime.Now;
            DetermineCurrentQuarter(bill, bill.DateCreated);
       
            return View(bill);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]  //[Bind(Include = "LastName, FirstMidName, EnrollmentDate")]Student student)
       // public async Task<IActionResult> Create(IFormCollection collection)
       public async Task<ActionResult> Create([Bind(include: "amount, billingCycle, clientName, title, idirOrUrl, serviceCategoryId, fiscalPeriod, quantity, ticketNumberAndRequester, dateModified, dateCreated, createdBy")] Bill bill)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _billRepository.CreateBill(bill); 
                }
            }
            catch (DbUpdateException )
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            
            return RedirectToAction(nameof(Index));
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
                if (!decimal.TryParse(category.Costs, out newAmount))
                {
                    newAmount = 0;
                }
                if (category.ServiceId == 5)
                    newAmount = 85;
                return new JsonResult(newAmount * quantity);
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
    }
}
