using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Index()
        {
            IEnumerable<Bill> bills = _billRepository.AllBills;
            IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();
            IEnumerable<ClientAccount> clients = _clientAccountRepository.GetAll();

            return View(new BillViewModel(bills, categories, clients));
        }
    }
}
