using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Service_Billing.Data;
using Service_Billing.Models;
using Service_Billing.Models.Repositories;
//using Service_Billing.ViewModels;
using System.Collections.Immutable;
using System.Data;
using Service_Billing.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
namespace Service_Billing.Controllers
{
    public class OrganizationController : Controller
    {
        private readonly ServiceBillingContext _billingContext;
        private readonly IMinistryRepository _ministryRepository;
        private readonly IClientAccountRepository _clientAccountRepository;
        private readonly ILogger<OrganizationController> _logger;
        private readonly IAuthorizationService _authorizationService;

        public OrganizationController(ILogger<OrganizationController> logger, IMinistryRepository ministryRepository,
            ServiceBillingContext billingContext,
            IClientAccountRepository clientAccountRepository,
            IAuthorizationService authorizationService)
        {
            _logger = logger;
            _ministryRepository = ministryRepository;
            _billingContext = billingContext;
            _clientAccountRepository = clientAccountRepository;
            _authorizationService = authorizationService;
        }

        [Authorize]
        [Authorize(Roles = "GDXBillingService.FinancialOfficer")]
        public IActionResult Index()
        {
            IEnumerable<Ministry> organizations = _ministryRepository.GetAll();
            return View(organizations);
        }

        [HttpGet]
        [Authorize]
        [Authorize(Roles = "GDXBillingService.FinancialOfficer")]
        public IActionResult Edit(int id)
        {
            try
            {
                Ministry? org = _ministryRepository.GetById(id);
                

                if (org == null)
                    return NotFound();

                return View(org);
            }
            catch (DbUpdateException ex)
            { 
                _logger.LogError(ex.Message);
            }

            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Ministry organization)
        {
            try
            {
                //do we need to update client account names?
                IEnumerable<ClientAccount> matchingAccounts = _clientAccountRepository.GetAccountsByOrgId(organization.Id);
                if(matchingAccounts.Any() && !matchingAccounts.First().Name.ToLower().StartsWith(organization.Acronym.ToLower()))
                {// Update
                    foreach(ClientAccount account in matchingAccounts)
                    {
                        if(!String.IsNullOrEmpty(account.Name))
                        {
                            string user = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "NAME NOT DETERMINED";
                            account.Name = account.Name.Replace(account.Name.Substring(0, account.Name.IndexOf(' ')), organization.Acronym);
                            await _clientAccountRepository.Update(account, user, false);
                        }
                    }
                    await _billingContext.SaveChangesAsync();
                }

                await _ministryRepository.Update(organization);
            }

            catch (DbUpdateException ex)
            {
                _logger.LogError(ex.Message);
            }

            IEnumerable<Ministry> organizations = _ministryRepository.GetAll();

            return View("index", organizations);
        }

        [HttpGet]
        public IActionResult Create()
        {
            Ministry model = new Ministry();
            return PartialView("_Create", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Ministry ministry)
        {
            try
            {
                await _ministryRepository.Save(ministry);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"A database update exception occurred: {ex.Message}");
            }
            IEnumerable<Ministry> organizations = _ministryRepository.GetAll();

            return View("index", organizations);
        }
    }
}
