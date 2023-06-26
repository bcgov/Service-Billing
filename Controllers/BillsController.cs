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
            //   ViewBag.Quarter = String.IsNullOrEmpty(quarter) ? "name_desc" : "";

            ViewBag.ServiceCategories = categories.ToList();
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
                if (!decimal.TryParse(category.costs, out newAmount))
                {
                    newAmount = 0;
                }
                if (category.serviceId == 5)
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
    }
}
