using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Service_Billing.Models;
using Service_Billing.ViewModels;

namespace Service_Billing.Controllers
{
    public class BillsController : Controller
    {
        private readonly IBillRepositroy _billRepository;
        private readonly IServiceCategoryRepository _categoryRepository;
        private readonly IClientAccountRepository _clientAccountRepository;

        public BillsController(IBillRepositroy billRepository,
            IServiceCategoryRepository categoryRepository,
            IClientAccountRepository clientAccountRepository)
        {
            _billRepository = billRepository;
            _categoryRepository = categoryRepository;
            _clientAccountRepository = clientAccountRepository;
        }

        public IActionResult Index(string quarter)
        {
            IEnumerable<Bill> bills;
            IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();
            IEnumerable<ClientAccount> clients = _clientAccountRepository.GetAll();
            ViewBag.ServiceCategories = categories.ToList();
            //   ViewBag.Quarter = String.IsNullOrEmpty(quarter) ? "name_desc" : "";

            switch (quarter)
            {
                case "current":
                default:
                    ViewBag.Quarter = "Current Quarter";
                    bills = _billRepository.GetCurrentQuarterBills();
                    break;
                case "previous":
                    ViewBag.Quarter = "Previous Quarter";
                    bills = _billRepository.GetPreviousQuarterBills();
                    break;
                case "all":
                    ViewBag.Quarter = "All Quarters";
                    bills = _billRepository.AllBills;
                    break;
            }
            var x = bills.Count();
            return View(new AllBillsViewModel(bills, categories, clients));
        }

        public ActionResult Details(int id)
        {
            Bill? bill = _billRepository.GetBill(id);
            if (bill == null)
            {
                return NotFound();
            }
            ClientAccount? account = _clientAccountRepository.GetClientAccount(bill.clientAccountId);
            ServiceCategory? serviceCategory = _categoryRepository.GetById(bill.serviceCategoryId);
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

            bill.dateModified = DateTime.Now;
            if (String.IsNullOrEmpty(bill.fiscalPeriod) || String.IsNullOrEmpty(bill.billingCycle))
                DetermineCurrentQuarter(bill, bill.dateCreated);
            ViewData["Client"] = _clientAccountRepository.GetClientAccount(bill.clientAccountId);
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
                b => b.clientAccountId,
                b => b.clientName,
                b => b.title,
                b => b.idirOrUrl,
                b => b.serviceCategoryId,
                b => b.amount,
                b => b.quantity,
                b => b.ticketNumberAndRequester,
                b => b.dateModified,
                b => b.createdBy
                ))
            {
                try
                {
                    await _billRepository.SaveChangesAsync();
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
            bill.dateCreated = DateTime.Now;
            DetermineCurrentQuarter(bill, bill.dateCreated);
       
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
                    bill.billingCycle = new DateTime(today.Year, 4, 1).ToString("yyyy-MM-dd");
                    break;
                case 7:
                case 8:
                case 9:
                    quarter = "Quarter 2";
                    bill.billingCycle = new DateTime(today.Year, 7, 1).ToString("yyyy-MM-dd");
                    break;
                case 10:
                case 11:
                case 12:
                    quarter = "Quarter 3";
                    bill.billingCycle = new DateTime(today.Year, 10, 1).ToString("yyyy-MM-dd");
                    break;
                case 1:
                case 2:
                case 3:
                    quarter = "Quarter 4";
                    bill.billingCycle = new DateTime(today.Year, 1, 1).ToString("yyyy-MM-dd");
                    break;
            }

            bill.fiscalPeriod = $"Fiscal {year1.Substring(2)}/{year2.Substring(2)} {quarter}";
        }
    }
}
