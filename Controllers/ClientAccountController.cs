using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Billing.Models;
using Service_Billing.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Service_Billing.Models.Repositories;
using CsvHelper;
using Microsoft.Identity.Client;
using Service_Billing.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Service_Billing.Extensions;
using Service_Billing.Filters;
using System.Net.Http.Headers;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Service_Billing.Services.GraphApi;
using Microsoft.Graph.TermStore;
using System.Security.Principal;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Vml;

namespace Service_Billing.Controllers
{
    public class ClientAccountController : Controller
    {
        private readonly IClientAccountRepository _clientAccountRepository;
        private readonly IMinistryRepository _ministryRepository;
        private readonly IBillRepository _billRepository;
        private readonly IServiceCategoryRepository _categoryRepository;
        private readonly GraphServiceClient? _graphServiceClient;
        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;
        private readonly ILogger<ClientAccountController> _logger;
        private readonly string[]? _graphScopes;
        private readonly IEmailService _emailService;
        private readonly IGraphApiService _graphApiService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IConfiguration _configuration;


        public ClientAccountController(ILogger<ClientAccountController> logger,
            IClientAccountRepository clientAccountRepository,
            IMinistryRepository ministryRepository,
            IBillRepository billRepository,
            IAuthorizationService authorizationService,
            IServiceCategoryRepository categoryRepository,
            IConfiguration configuration,
                            GraphServiceClient graphServiceClient,
                            MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler,
                            IEmailService emailService,
                            IGraphApiService graphApiService)
        {
            _graphServiceClient = graphServiceClient;
            _consentHandler = consentHandler;

            _clientAccountRepository = clientAccountRepository;
            _ministryRepository = ministryRepository;
            _billRepository = billRepository;
            _categoryRepository = categoryRepository;
            _logger = logger;
            _graphScopes = configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');
            _emailService = emailService;
            _graphApiService = graphApiService;
            _authorizationService = authorizationService;
            _configuration = configuration;
        }

        // GET: ClientAccountController
        [Authorize]
        [Authorize(Roles = "GDXBillingService.FinancialOfficer, GDXBillingService.Owner, GDXBillingService.User")]
        public async Task<ActionResult> Index(int ministryFilter, int numberFilter, string responsibilityFilter, string authorityFilter, string teamFilter, string keyword, string primaryContactFilter)
        {
            // TODO: Add filtering options or Services Enabled and Notes
            // "Add “Notes” field (this section will allow admins to update to identify service ticket number or changes made to client account)"
            IEnumerable<Ministry> ministries = _ministryRepository.GetAll();
            ViewData["Ministries"] = ministries;
            ViewData["MinistryFilter"] = ministryFilter;
            ViewData["NumberFilter"] = numberFilter;
            ViewData["AuthorityFilter"] = authorityFilter;
            ViewData["PrimaryContactFilter"] = primaryContactFilter;
            ViewData["ResponsibilityFilter"] = responsibilityFilter;
            ViewData["TeamFilter"] = teamFilter;
            ViewData["Keyword"] = keyword;

            var isMinistryUser = User.IsInRole("GDXBillingService.User");
            string? ministryUserName = string.Empty;
            if (isMinistryUser) ministryUserName = User?.FindFirst("name")?.Value;

            IEnumerable<ClientAccount> clients = GetFilteredAccounts(ministryFilter, numberFilter, responsibilityFilter, authorityFilter, teamFilter, keyword, primaryContactFilter, ministryUserName ?? "");
 
            return View(clients);
        }


        // GET: ClientAccountController/Details/5
        [Authorize(Roles = "GDXBillingService.FinancialOfficer, GDXBillingService.Owner, GDXBillingService.User")]
        public ActionResult Details(int id)
        {
            ClientAccount? account = _clientAccountRepository.GetClientAccount(id);
            if (account == null)
                return NotFound();

            // check if user ought to be able to view this record
            if (!User.IsInRole("GDXBillingService.FinancialOfficer"))
            {
                string userLastName = GetUserLastName();
                if (!String.IsNullOrEmpty(userLastName))
                {
                    if (!IsUserAccountContact(account, userLastName))
                    {
                        if (!String.IsNullOrEmpty(account.ExpenseAuthorityName) && !account.ExpenseAuthorityName.ToLower().Contains(userLastName.ToLower()))
                        {

                            return View("Unauthorized");
                        }
                    }
                }
                else
                {
                    _logger.LogWarning($"User tried to view Account details for client account with id: {id}, but user's name could not be discerned");
                    return View("Unauthorized");
                }
            }
            IEnumerable<Bill> charges = _billRepository.GetBillsByClientId(id);
            IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();

            return View(account);
        }

        // GET: ClientAccountController/Edit/5
        public ActionResult Edit(int id)
        {
            ClientAccount? account = _clientAccountRepository.GetClientAccount(id);

            if (account == null)
                return NotFound();
        
            return View(account);
        }

        // POST: ClientAccountController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ClientAccount model)
        {
            try
            {
                _clientAccountRepository.Update(model);

                return RedirectToAction("Details", new { model.Id });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"An error occurred while updating client account number {model.Id}. Inner Exception: {ex.InnerException}");
                _logger.LogError(ex.Message);
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
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return View();
            }
        }

        [ServiceFilter(typeof(GroupAuthorizeActionFilter))]
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            _logger.LogInformation("User visited Intake form.");
            IEnumerable<Ministry> ministries = _ministryRepository.GetAll();
            ViewData["Ministries"] = ministries;
            ClientCreateViewModel model = new ClientCreateViewModel();

            return View(model);
        }

        [ServiceFilter(typeof(GroupAuthorizeActionFilter))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientCreateViewModel model)
        {
            _logger.LogInformation("User submitted Intake form.");
            try
            {
                string accountName = $"{model.MinistryAcronym} - {model.DivisionOrBranch}";
                model.Account.Name = accountName;
                ClientAccount account = model.Account;
               
                _logger.LogInformation($"Client Account with Id: {account.Id} is being added to DB");

                int accountId = _clientAccountRepository.AddClientAccount(account);

                var cca = ConfidentialClientApplicationBuilder
                   .Create(_configuration.GetSection("AzureAd")["ClientId"])
                   .WithClientSecret(_configuration.GetSection("AzureAd")["ClientSecret"])
                   .WithAuthority(new Uri($"https://login.microsoftonline.com/{_configuration.GetSection("AzureAd")["TenantId"]}"))
                    .Build();

                var expenseAuthority = await _graphApiService.GetUsersByDisplayName(account.ExpenseAuthorityName, cca);
                if (expenseAuthority is not null)
                {
                    var eaId = expenseAuthority?.Value?.FirstOrDefault()?.Id;
                    var eaEmail = (await _graphApiService.Me(eaId, cca)).UserPrincipalName;
                    var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

                    //await _emailService.SendEmail(
                    //    eaEmail,
                    //    "Please Review: New GDX Service Billing Account Information",
                    //    $@"
                    //    Hello {eaEmail.Split('@')[0]},
                    
                    //    We hope this message finds you well. We're writing to inform you that a new account has been created for you in the GDX Service Billing system, designed to enhance your access and features.
                    
                    //    To complete the setup of your account, please verify its creation. We prioritize your security and do not include direct links in our emails. You can safely access the GDX Service Billing portal through our official website or your internal systems.
                    
                    //    If this account was not requested by you or if you believe you have received this email by mistake, please get in touch with our support team at [Support Contact Information] for immediate assistance.
                    
                    //    We appreciate your attention to this matter. Should you have any questions or require further assistance, do not hesitate to contact us.
                    
                    //    Warm regards,
                    
                    //    GDX Service Billing Team"
                    //);
                }

            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error adding client account to DB. Inner Exception: {ex.InnerException}");
                return View(new ClientCreateViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occured while trying to add a client account: {ex.InnerException}");
            }

            IEnumerable<Bill> charges = _billRepository.GetBillsByClientId(model.Account.Id);
            IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();

            return RedirectToAction("details", new { model.Account.Id });
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

                //await _emailService.SendEmail("waino.steuber35@ethereal.email",
                //      $"Account {id} approved",
                //       $"<p>Account {id} has been approved.</p>");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return View();
            }
        }

        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public async Task<IActionResult> SearchForContact(string term)
        {
            try
            {
                var cca = ConfidentialClientApplicationBuilder
                    .Create(_configuration.GetSection("AzureAd")["ClientId"])
                    .WithClientSecret(_configuration.GetSection("AzureAd")["ClientSecret"])
                    .WithAuthority(new Uri($"https://login.microsoftonline.com/{_configuration.GetSection("AzureAd")["TenantId"]}"))
                    .Build();

                var queriedUsers = await _graphApiService.GetUsersByDisplayName(term, cca);

                List<SelectListItem> contactItems = new List<SelectListItem>();
                List<string> contacts = new List<string>();
                if(queriedUsers.Value != null)
                {
                    foreach (var user in queriedUsers.Value)
                    {
                        contacts.Add(user.DisplayName);
                    }
                }

                return new JsonResult(contacts);
            }
            catch (ServiceException svcex)
            {
                _logger.LogError("THIS IS THE SERVICE EXCEPTION!!!");
                _logger.LogWarning(svcex.Message);

                string[] scopes = { "user.read", "user.readbasic.all" };
                _consentHandler.ChallengeUser(scopes);
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                _logger.LogError($"An exception occurred while trying to fetch graph data: \n {ex.Message}");
                _logger.LogError($"TokenSource: /n IdentProvider: {TokenSource.IdentityProvider}" +
                   $" \n:Broker: {TokenSource.Broker} \n  Cache: {TokenSource.Cache}");
                _consentHandler.HandleException(ex);
                return new JsonResult(ex.InnerException);
            }
        }

        public IActionResult CurrentUserAccounts()
        {
            if (_graphServiceClient == null)
                throw new Exception("GraphServiceClient is null in BillsController.CurrentUserAccounts");
            User currentUser = _graphServiceClient.Me.Request().GetAsync().Result;
            IEnumerable<ClientAccount> currentUserAccounts = _clientAccountRepository.GetAccountsByContactName(currentUser.DisplayName);

            return View("Index", new ClientAccountViewModel(currentUserAccounts));
        }

        private IEnumerable<ClientAccount> GetFilteredAccounts(int ministryFilter, int numberFilter, string responsibilityFilter, string authorityFilter, string teamFilter, string keyword, string primaryContactFilter, string ministryUserName = "")
        {
            IEnumerable<ClientAccount> clients = _clientAccountRepository.GetAll();
            if (ministryFilter > 0)
                clients = clients.Where(x => x.OrganizationId != null && x.OrganizationId == ministryFilter);
            if (numberFilter > 0)
                clients = clients.Where(x => x.Id == numberFilter).ToList();
            if (!String.IsNullOrEmpty(responsibilityFilter))
                clients = clients.Where(x => !String.IsNullOrEmpty(x.ResponsibilityCentre) && x.ResponsibilityCentre.ToLower().Contains(responsibilityFilter.ToLower()));
            if (!String.IsNullOrEmpty(authorityFilter))
                clients = clients.Where(x => !String.IsNullOrEmpty(x.ExpenseAuthorityName) && x.ExpenseAuthorityName.ToLower().Contains(authorityFilter.ToLower()));
            //if (!String.IsNullOrEmpty(teamFilter))
            //    clients = clients.Where(x => !String.IsNullOrEmpty(x.ClientTeam) && x.ClientTeam.ToLower().Contains(teamFilter.ToLower()));
            if (!String.IsNullOrEmpty(keyword))
            {
                clients = clients.Where(x => (!String.IsNullOrEmpty(x.Name) && x.Name.ToLower().Contains(keyword.ToLower())) ||
                (!String.IsNullOrEmpty(x.ResponsibilityCentre) && x.ResponsibilityCentre.ToLower().Contains(keyword.ToLower()) ||
                (!String.IsNullOrEmpty(x.Project) && x.Project.ToLower().Contains(keyword.ToLower())) ||
                (!String.IsNullOrEmpty(x.ServicesEnabled) && x.ServicesEnabled.ToLower().Contains(keyword.ToLower())) ||
                (!String.IsNullOrEmpty(x.ExpenseAuthorityName) && x.ExpenseAuthorityName.ToLower().Contains(keyword.ToLower())))
                // || (!String.IsNullOrEmpty(x.ClientTeam) && x.ClientTeam.ToLower().Contains(keyword.ToLower())))
                );
            }

            if (!String.IsNullOrEmpty(ministryUserName))
            {
                clients = clients.Where(x => !String.IsNullOrEmpty(x.ExpenseAuthorityName) && x.ExpenseAuthorityName.ToLower().Contains(ministryUserName.ToLower()));
            }
            if(!String.IsNullOrEmpty(primaryContactFilter))
                clients = clients.Where(x => !String.IsNullOrEmpty(x.PrimaryContact) &&  x.PrimaryContact.ToLower().Contains(primaryContactFilter.ToLower()));

            return clients;
        }

        [HttpGet]
        public IActionResult WriteToExcel(int ministryFilter, int numberFilter, string responsibilityFilter, string authorityFilter, string teamFilter, string keyword, string primaryContactFilter)
        {
            IEnumerable<ClientAccount> accounts = GetFilteredAccounts(ministryFilter, numberFilter, responsibilityFilter, authorityFilter, teamFilter, keyword, primaryContactFilter);
            try
            {
                string fileName = "Client-Accounts";
                if (ministryFilter > 0)
                {
                    Ministry? ministry = _ministryRepository.GetById(ministryFilter);
                    if(ministry != null)
                        fileName += $"-Client{ministry.Title}";
                }
                if (!String.IsNullOrEmpty(responsibilityFilter))
                    fileName += $"-{responsibilityFilter}";
                if (!String.IsNullOrEmpty(authorityFilter))
                    fileName += $"-{authorityFilter}";
                if (!String.IsNullOrEmpty(teamFilter))
                    fileName += $"-{teamFilter}";

                fileName += DateTime.Today.ToString("dd-mm-yyyy");
                fileName += ".xlsx";

                List<ClientInfoRowEntry> rows = new List<ClientInfoRowEntry>();

                foreach(ClientAccount account in accounts)
                {
                    ClientInfoRowEntry row = new ClientInfoRowEntry(account);
                    if (account.OrganizationId != null && account.OrganizationId.HasValue)
                    {
                        row.organization = _ministryRepository.GetById(account.OrganizationId.Value).Title;
                    }
                    rows.Add(row);
                }

                using var wb = new XLWorkbook();
                var ws = wb.AddWorksheet();
                ws.Cell("A1").InsertTable(rows);
                // Adjust column size to contents.
                ws.Columns().AdjustToContents();
                using var stream = new MemoryStream();
                wb.SaveAs(stream);
                var content = stream.ToArray();
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(content, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                //we really need an error page
                return StatusCode(500);
            }
        }

        [ServiceFilter(typeof(GroupAuthorizeActionFilter))]
        [HttpPost]
        [Authorize(Roles = "GDXBillingService.FinancialOfficer")]
        public IActionResult SetIsActiveForClient(int id, bool active)
        {
            try
            {


                ClientAccount? account = _clientAccountRepository.GetClientAccount(id);
                if (account != null)
                {
                    account.IsActive = active;
                    if (!active) //deactivate all charges associates with this client;
                    {
                        IEnumerable<Bill> charges = _billRepository.GetBillsByClientId(id);
                        foreach (Bill charge in charges)
                        {
                            charge.IsActive = false;
                            _billRepository.Update(charge);
                        }
                    }
                    _clientAccountRepository.Update(account);
                }
                else
                {
                    _logger.LogError($"Admin user tried to deactivate bill with id {id}, but it was not found in database");
                    return BadRequest();
                }
                return Ok(200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return StatusCode(500);
            }

        }


        // Right now this just checks if the current user has a last name that matches a contact.
        // It could certainly be improved by having all contact entries match their Azure AD display name,
        // like "Alexander.Carmichael@gov.bc.ca
        public bool IsUserAccountContact(ClientAccount account, string lastName)
        {
            if (!String.IsNullOrEmpty(lastName))
            {
                if (!String.IsNullOrEmpty(lastName) &&
                    (!String.IsNullOrEmpty(account.PrimaryContact) && account.PrimaryContact.ToLower().Contains(lastName.ToLower())) ||
                    (!String.IsNullOrEmpty(account.FinancialContact) && account.FinancialContact.ToLower().Contains(lastName.ToLower())) ||
                    (!String.IsNullOrEmpty(account.Approver) && account.Approver.ToLower().Contains(lastName.ToLower())))
                {
                    return true;
                }
            }

            return false;
        }

        public string GetUserLastName()
        {
            string? userName = User.GetDisplayName();
            string lastName = string.Empty;
            if (!String.IsNullOrEmpty(userName))
            {
                string[] nameElements = userName.Split('.');
                if (nameElements.Length > 1)
                {
                    lastName = nameElements[1].Substring(0, nameElements[1].IndexOf('@'));
                    lastName = lastName.Trim();
                }
            }
            return lastName;
        }
    }

    public class ClientInfoRowEntry
    {
        public int clientId;
        public string clientName;
        public string organization;
        public string aggregateGLCode;
        public string servicesEnabled;
        public string primaryContact;
        public string financialContact;
        public string approver;
        public string expenseAuthority;
        public bool approved;
        public bool active;
        public string notes;




        public ClientInfoRowEntry(ClientAccount account)
        {
            clientId = account.Id;
            clientName = account.Name;
            primaryContact = account.PrimaryContact;
            financialContact = account.FinancialContact;
            approver = account.Approver;
            approved = account.IsApprovedByEA;
            active = account.IsActive;
            aggregateGLCode = account.AggregatedGLCode;
            notes = account.Notes;
        }
    }
}
