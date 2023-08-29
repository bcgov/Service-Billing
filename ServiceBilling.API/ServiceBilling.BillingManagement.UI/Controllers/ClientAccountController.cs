using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceBilling.BillingManagement.UI.Models;
using ServiceBilling.BillingManagement.UI.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceBilling.BillingManagement.UI.Models;
using ServiceBilling.BillingManagement.UI.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using ServiceBilling.BillingManagement.UI.Models.Repositories;

namespace ServiceBilling.BillingManagement.UI.Controllers
{
    public class ClientAccountController: Controller
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;
        private readonly IClientAccountRepository _clientAccountRepository;
        private readonly IClientTeamRepository _clientTeamRepository;
        private readonly IMinistryRepository _ministryRepository;
        private readonly IServiceCategoryRepository _categoryRepository;

        public ClientAccountController(IConfiguration configuration,
            IClientTeamRepository clientTeamRepository,
            IMinistryRepository ministryRepository,
            IServiceCategoryRepository categoryRepository,
            IClientAccountRepository clientAccountRepository,
                            GraphServiceClient graphServiceClient,
                            MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
        {
            _graphServiceClient = graphServiceClient;
            _consentHandler = consentHandler;
            _categoryRepository = categoryRepository;
            _clientAccountRepository = clientAccountRepository;
            _ministryRepository = ministryRepository;
        }

        public ActionResult Index(string ministryFilter, int numberFilter, string responsibilityFilter, string authorityFilter, string teamFilter)
        {
            IEnumerable<ClientAccount> clients = _clientAccountRepository.GetAll();
            IEnumerable<Ministry> ministries = _ministryRepository.GetAll();
            ViewData["Ministries"] = ministries;
            ViewData["MinistryFilter"] = ministryFilter;
            ViewData["NumberFilter"] = numberFilter;
            ViewData["AuthorityFilter"] = authorityFilter;
            ViewData["ResponsibilityFilter"] = responsibilityFilter;
            ViewData["TeamFilter"] = teamFilter;

            if (!String.IsNullOrEmpty(ministryFilter))
                clients = clients.Where(x => !String.IsNullOrEmpty(x.Name) && x.Name.Contains(ministryFilter)).ToList();
            if (numberFilter > 0)
                clients = clients.Where(x => x.ClientNumber == numberFilter).ToList();
            if (!String.IsNullOrEmpty(responsibilityFilter))
                clients = clients.Where(x => !String.IsNullOrEmpty(x.ResponsibilityCentre) && x.ResponsibilityCentre.Contains(responsibilityFilter)).ToList();
            if (!String.IsNullOrEmpty(authorityFilter))
                clients = clients.Where(x => !String.IsNullOrEmpty(x.ExpenseAuthorityName) && x.ExpenseAuthorityName.Contains(authorityFilter)).ToList();
            if (!String.IsNullOrEmpty(teamFilter))
                clients = clients.Where(x => !String.IsNullOrEmpty(x.ClientTeam) && x.ClientTeam.Contains(teamFilter)).ToList();

            return View(new ClientAccountViewModel(clients));
        }
        public ActionResult Details(int id)
        {
            ClientAccount? account = _clientAccountRepository.GetClientAccount(id);
            if (account == null)
                return NotFound();
            ClientTeam? team = _clientTeamRepository.GetTeamById(account.TeamId);
            ViewData["clientTeam"] = team != null ? team : "";
            return View(account);
        }

        // GET: ClientAccountController/Edit/5
        public ActionResult Edit(int id)
        {
            var account = _clientAccountRepository.GetClientAccount(id);
            if (account == null)
                return NotFound();
            return View(account);
        }

        // POST: ClientAccountController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormCollection collection)
        {
            if (ModelState.IsValid)
            {
                ClientAccount? accountToUpdate = _clientAccountRepository.GetClientAccount(id);
                if (accountToUpdate == null)
                {
                    return NotFound();
                }
                if (await TryUpdateModelAsync<ClientAccount>(accountToUpdate, "",
                    a => a.Name,
                    a => a.ClientNumber,
                    a => a.ResponsibilityCentre,
                    a => a.ServiceLine,
                    a => a.STOB,
                    a => a.Project,
                    a => a.ExpenseAuthorityName,
                    a => a.ServicesEnabled))
                {
                    try
                    {
                        _clientAccountRepository.AddClientAccount(accountToUpdate);
                    }
                    catch (DbUpdateException /* ex */)
                    {
                        ModelState.AddModelError("", "Unable to save changes. " +
                            "Try again, and if the problem persists, " +
                            "see your system administrator.");
                    }

                    return RedirectToAction(nameof(Index));
                }
            }

            return Details(id);
        }

        [HttpGet]
        public ActionResult Intake()
        {
            IEnumerable<Ministry> ministries = _ministryRepository.GetAll();
            ViewData["Ministries"] = ministries;

            return View(new ClientIntakeViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Intake(ClientIntakeViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string accountName = $"{model.MinistryAcronym} - {model.DivisionOrBranch}";
                    model.Account.Name = accountName;
                    ClientTeam team = model.Team;
                    team.Name = model.Account.Name;
                    int teamId = _clientTeamRepository.Add(team);
                    ClientAccount account = model.Account;
                    account.TeamId = teamId;
                    int accountId = _clientAccountRepository.AddClientAccount(account);
                }
            }
            catch (DbUpdateException)
            {
                return View(new ClientIntakeViewModel());
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public async Task<IActionResult> SearchForContact(string query, string contactType, ClientIntakeViewModel model)
        {
            try
            {
                var queriedUsers = await _graphServiceClient.Users.Request()
                    .Filter($"startswith(displayName, '{query}')")
                    .Top(5)
                    .Select("displayName, id")
                    .GetAsync();

                List<SelectListItem> contactItems = new List<SelectListItem>();
                foreach (var user in queriedUsers)
                {
                    contactItems.Add(new SelectListItem(user.DisplayName, user.DisplayName));
                }

                model.Contacts = contactItems;

                return ViewComponent("ContactLookup", new { elementId = contactType.Replace("Select", "Value"), contactList = contactItems, model = model });
            }
            catch (Exception ex)
            {
                return new JsonResult(ex);
            }
        }
    }
}
