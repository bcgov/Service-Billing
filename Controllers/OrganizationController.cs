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
namespace Service_Billing.Controllers
{
    public class OrganizationController : Controller
    {
        private readonly ServiceBillingContext _billingContext;
        private readonly IMinistryRepository _ministryRepository;
        private readonly ILogger<OrganizationController> _logger;
        private readonly IAuthorizationService _authorizationService;

        public OrganizationController(ILogger<OrganizationController> logger, IMinistryRepository ministryRepository, ServiceBillingContext billingContext,
            IAuthorizationService authorizationService)
        {
            _logger = logger;
            _ministryRepository = ministryRepository;
            _billingContext = billingContext;
            _authorizationService = authorizationService;
        }

        [Authorize]
        [Authorize(Roles = "GDXBillingService.FinancialOfficer")]
        public IActionResult Index()
        {
            IEnumerable<Ministry> organizations = _ministryRepository.GetAll();
            return View(organizations);
        }
    }
}
