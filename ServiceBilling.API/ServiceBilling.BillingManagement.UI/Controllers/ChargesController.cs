using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using ServiceBilling.BillingManagement.UI.Models;
using ServiceBilling.BillingManagement.UI.Models.Repositories;
using ServiceBilling.BillingManagement.UI.ViewModels;

namespace ServiceBilling.BillingManagement.UI.Controllers
{

    public class ChargesController : Controller
    {
        private readonly IChargesRepository _chargeRepository;
        private readonly IServiceCategoryRepository _categoryRepository;
        private readonly IClientAccountRepository _clientAccountRepository;
        private readonly IMinistryRepository _ministryRepository;
        private readonly GraphServiceClient _graphServiceClient;
        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;

        public ChargesController(IChargesRepository chargeRepository,
            IServiceCategoryRepository categoryRepository,
            IClientAccountRepository clientAccountRepository,
            IMinistryRepository ministryRepository,
            IConfiguration configuration,
                            GraphServiceClient graphServiceClient,
                            MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
        {
            _graphServiceClient = graphServiceClient;
            _consentHandler = consentHandler;
            _chargeRepository = chargeRepository;
            _categoryRepository = categoryRepository;
            _clientAccountRepository = clientAccountRepository;
            _ministryRepository = ministryRepository;
        }
        public IActionResult Index(string quarterFilter,
            string ministryFilter,
            string titleFilter,
            Guid? categoryFilter,
            string authorityFilter,
            bool meFilter,
            int clientNumber)
        {
            IEnumerable<Charge> charges;
            IEnumerable<ServiceCategory> categories = _categoryRepository.GetAllServiceCategories();
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
                    charges = _chargeRepository.GetCurrentQuarterCharges();
                    break;
                case "previous":
                    ViewData["Quarter"] = "Previous Quarter";
                    charges = _chargeRepository.GetPreviousQuarterCharges();
                    break;
                case "all":
                    ViewData["Quarter"] = "All Quarters";
                    charges = _chargeRepository.AllCharges;
                    break;
            }
            // now filter the results
            if (!string.IsNullOrEmpty(ministryFilter))
                charges = charges.Where(x => x.ClientName.ToLower().Contains(ministryFilter.ToLower()));
            if (!string.IsNullOrEmpty(titleFilter))
                charges = charges.Where(x => x.Title.ToLower().Contains(titleFilter.ToLower()));
            if (categoryFilter != null)
                charges = charges.Where(x => x.ServiceCategoryId == categoryFilter);
            if (!string.IsNullOrEmpty(authorityFilter))
            {
                List<Charge> filteredCharges = new List<Charge>();
                foreach (Charge charge in charges)
                {
                    ClientAccount? account = _clientAccountRepository.GetClientAccount(charge.ClientAccountId);
                    if (account != null && !String.IsNullOrEmpty(account.ExpenseAuthorityName) && account.ExpenseAuthorityName.Contains(authorityFilter))
                    {
                        filteredCharges.Append(charge);
                    }
                }

                charges = filteredCharges;
            }
            if (clientNumber > 0)
            {
                int clientId = _clientAccountRepository.GetClientIdFromClientNumber(clientNumber);
                charges = charges.Where(x => x.ClientAccountId == clientId);
            }

            return View(new AllChargesViewModel(charges, categories, clients));
        }
    }
}
