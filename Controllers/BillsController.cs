using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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

            switch (quarter)
            {
                case "current": default:
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

            return View(new BillViewModel(bills, categories, clients));
        }

        public ActionResult Details(int id)
        {
            Bill? bill = _billRepository.GetBill(id);
            if(bill == null)
            {
                return NotFound();
            }
            ClientAccount? account = _clientAccountRepository.GetClientAccount(bill.clientAccountId);
            ServiceCategory? serviceCategory = _categoryRepository.GetById(bill.serviceCategoryId);
            ViewData["clientAccount"] = account != null ? account : "";
            ViewData["serviceCategory"] = serviceCategory != null ? serviceCategory : "";
            return View(bill);
        }
    }
}
