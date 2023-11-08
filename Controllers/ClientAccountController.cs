using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Billing.Models;
using Service_Billing.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Service_Billing.Models.Repositories;
using CsvHelper;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Graph.TermStore;
using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Identity.Client;
using Service_Billing.Services.Email;

namespace Service_Billing.Controllers
{
    public class ClientAccountController : Controller
    {
        private readonly IClientAccountRepository _clientAccountRepository;
        private readonly IClientTeamRepository _clientTeamRepository;
        private readonly IMinistryRepository _ministryRepository;
        private readonly GraphServiceClient _graphServiceClient;
        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;
        private readonly ILogger<ClientAccountController> _logger;
        private readonly IEmailService _emailService;

        public ClientAccountController(ILogger<ClientAccountController> logger,
            IClientAccountRepository clientAccountRepository,
            IClientTeamRepository clientTeamRepository,
            IMinistryRepository ministryRepository,
            IConfiguration configuration,
                            GraphServiceClient graphServiceClient,
                            MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler,
                            IEmailService emailService)
        {
            _graphServiceClient = graphServiceClient;
            _consentHandler = consentHandler;

            _clientAccountRepository = clientAccountRepository;
            _clientTeamRepository = clientTeamRepository;
            _ministryRepository = ministryRepository;
            _logger = logger;
            _emailService = emailService;
        }

        // GET: ClientAccountController
        public ActionResult Index(string ministryFilter, int numberFilter, string responsibilityFilter, string authorityFilter, string teamFilter)
        {

            IEnumerable<Ministry> ministries = _ministryRepository.GetAll();
            ViewData["Ministries"] = ministries;
            ViewData["MinistryFilter"] = ministryFilter;
            ViewData["NumberFilter"] = numberFilter;
            ViewData["AuthorityFilter"] = authorityFilter;
            ViewData["ResponsibilityFilter"] = responsibilityFilter;
            ViewData["TeamFilter"] = teamFilter;
            IEnumerable<ClientAccount> clients = GetFilteredAccounts(ministryFilter, numberFilter, responsibilityFilter, authorityFilter, teamFilter);

            return View(new ClientAccountViewModel(clients));
        }

        // GET: ClientAccountController/Details/5
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
            ClientAccount? account = _clientAccountRepository.GetClientAccount(id);

            if (account == null)
                return NotFound();
            ClientTeam? team = _clientTeamRepository.GetTeamById(account.TeamId);
            ClientIntakeViewModel model = new ClientIntakeViewModel();
            model.Account = account;
            if (team != null)
            {
                model.Team = team;
            }
            else
            {
                _logger.LogWarning($"No client team was found for client account with ID {account.Id}.");
                model.Team = new ClientTeam();
            }

            return View(model);
        }

        // POST: ClientAccountController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClientIntakeViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.Team != null)
                    {
                        if (model.Team.Id == 0)
                        {
                            model.Team.Name = $"{model.Account.Name} Team";
                            model.Account.TeamId = _clientTeamRepository.Add(model.Team);
                            model.Account.ClientTeam = model.Team.Name;
                        }
                        else
                        {
                            _clientTeamRepository.Update(model.Team);
                            model.Account.TeamId = model.Team.Id;
                            model.Account.ClientTeam = model.Team.Name;
                        }
                    }
                    _clientAccountRepository.Update(model.Account);
                    return View("Details", model.Account);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError($"An error occurred while updating client account number {model.Account.Id}. Inner Exception: {ex.InnerException}");
                    _logger.LogError(ex.Message);
                }
            }
            return View(model);
        }

        // GET: ClientAccountController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ClientAccountController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult Intake()
        {
            _logger.LogInformation("User visited Intake form.");
            IEnumerable<Ministry> ministries = _ministryRepository.GetAll();
            ViewData["Ministries"] = ministries;
            ClientIntakeViewModel model = new ClientIntakeViewModel();
            model.Account.ClientNumber = GetNextClientNumber();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Intake(ClientIntakeViewModel model)
        {
            _logger.LogInformation("User submitted Intake form.");
            try
            {

                if (ModelState.IsValid)
                {
                    _logger.LogInformation("Intake form is valid.");
                    string accountName = $"{model.MinistryAcronym} - {model.DivisionOrBranch}";
                    model.Account.Name = accountName;
                    ClientAccount account = model.Account;
                    if (model.Team != null)
                    {
                        ClientTeam team = model.Team;
                        team.Name = $"{model.Account.Name} Team";
                        int teamId = _clientTeamRepository.Add(team);
                        account.TeamId = teamId;
                        account.ClientTeam = team.Name;
                        account.IsApprovedByEA = false;
                    }
                    _logger.LogInformation($"Client Account with client number {account.ClientNumber} is being added to DB");

                    int accountId = _clientAccountRepository.AddClientAccount(account);
                    var baseUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
                    await _emailService.SendEmail("waino.steuber35@ethereal.email",
                        "New account created",
                        $"<p><a href='{baseUrl}/ClientAccount/Approve/{accountId}'>Click here</a> to approve the account.</p>");
                }
                else
                {
                    return View(model);
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error adding client account to DB. Inner Exception: {ex.InnerException}");
                return View(new ClientIntakeViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occured while trying to add a client account: {ex.InnerException}");
            }

            return RedirectToAction(nameof(Index));
        }

        private short GetNextClientNumber()
        {
            short ret = 2054;
            IEnumerable<ClientAccount> accounts = _clientAccountRepository.GetAll();
            if (accounts != null && accounts.Any())
            {
                ret = (short)accounts.Count();

                while (accounts.FirstOrDefault(a => a.ClientNumber == ret) != null)
                    ret++;
            }

            return ret;
        }

        public ActionResult Approve(int id)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Approve(int id, IFormCollection collection)
        {
            try
            {
                ClientAccount? account = _clientAccountRepository.GetClientAccount(id);
                if (account == null)
                    return NotFound();
                _clientAccountRepository.Approve(account);

                await _emailService.SendEmail("waino.steuber35@ethereal.email",
                      $"Account {id} approved",
                       $"<p>Account {id} has been approved.</p>");

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpGet]
        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public async Task<IActionResult> SearchForContact(string term)
        {
            try
            {
                var queriedUsers = await _graphServiceClient.Users.Request()
                    .Filter($"startswith(displayName, '{term}')")
                    .Top(8)
                    .Select("displayName, id")
                    .GetAsync();

                List<SelectListItem> contactItems = new List<SelectListItem>();
                List<string> contacts = new List<string>();

                foreach (var user in queriedUsers)
                {
                    contacts.Add(user.DisplayName);
                }

                return new JsonResult(contacts);
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.InnerException);
            }
        }

        public async Task<IActionResult> CurrentUserAccounts()
        {
            User currentUser = _graphServiceClient.Me.Request().GetAsync().Result;
            IEnumerable<ClientAccount> currentUserAccounts = _clientAccountRepository.GetAccountsByContactName(currentUser.DisplayName);

            return View("Index", new ClientAccountViewModel(currentUserAccounts));
        }

        private IEnumerable<ClientAccount> GetFilteredAccounts(string ministryFilter, int numberFilter, string responsibilityFilter, string authorityFilter, string teamFilter)
        {
            IEnumerable<ClientAccount> clients = _clientAccountRepository.GetAll();
            if (!String.IsNullOrEmpty(ministryFilter))
                clients = clients.Where(x => !String.IsNullOrEmpty(x.Name) && x.Name.Contains(ministryFilter));
            if (numberFilter > 0)
                clients = clients.Where(x => x.ClientNumber == numberFilter).ToList();
            if (!String.IsNullOrEmpty(responsibilityFilter))
                clients = clients.Where(x => !String.IsNullOrEmpty(x.ResponsibilityCentre) && x.ResponsibilityCentre.ToLower().Contains(responsibilityFilter.ToLower()));
            if (!String.IsNullOrEmpty(authorityFilter))
                clients = clients.Where(x => !String.IsNullOrEmpty(x.ExpenseAuthorityName) && x.ExpenseAuthorityName.ToLower().Contains(authorityFilter.ToLower()));
            if (!String.IsNullOrEmpty(teamFilter))
                clients = clients.Where(x => !String.IsNullOrEmpty(x.ClientTeam) && x.ClientTeam.ToLower().Contains(teamFilter.ToLower()));

            return clients;
        }

        [HttpGet]
        public async Task<IActionResult> WriteToCSV(string ministryFilter, int numberFilter, string responsibilityFilter, string authorityFilter, string teamFilter)
        {
            IEnumerable<ClientAccount> accounts = GetFilteredAccounts(ministryFilter, numberFilter, responsibilityFilter, authorityFilter, teamFilter);
            try
            {
                using var memoryStream = new MemoryStream();
                using (var streamWriter = new StreamWriter(memoryStream))
                {
                    using var csvWriter = new CsvWriter(streamWriter);
                    csvWriter.WriteRecords(accounts);
                }
                string fileName = "Client-Accounts";
                if (!String.IsNullOrEmpty(ministryFilter))
                    fileName += $"-Client{numberFilter}";
                if (!String.IsNullOrEmpty(responsibilityFilter))
                    fileName += $"-{responsibilityFilter}";
                if (!String.IsNullOrEmpty(authorityFilter))
                    fileName += $"-{authorityFilter}";
                if (!String.IsNullOrEmpty(teamFilter))
                    fileName += $"-{teamFilter}";


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
    }
}
