using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Billing.Models;
using Service_Billing.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Service_Billing.Models.Repositories;
using Microsoft.Identity.Client;
using Service_Billing.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Service_Billing.Filters;
using Service_Billing.Services.GraphApi;
using ClosedXML.Excel;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Office2019.Drawing.Model3D;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Razor.TagHelpers;
using DocumentFormat.OpenXml.InkML;

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
        private readonly IBusinessAreaRepository _businessAreaRepository;
        private readonly IPeopleRepository _peopleRepository;
        private readonly IContactRepository _contactRepository;
        private readonly IConfidentialClientApplication cca;


        public ClientAccountController(ILogger<ClientAccountController> logger,
            IClientAccountRepository clientAccountRepository,
            IMinistryRepository ministryRepository,
            IBillRepository billRepository,
            IAuthorizationService authorizationService,
            IServiceCategoryRepository categoryRepository,
            IBusinessAreaRepository businessAreaRepository,
            IPeopleRepository peopleRepository,
            IContactRepository contactRepository,
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
            _businessAreaRepository = businessAreaRepository;
            _peopleRepository = peopleRepository;
            _contactRepository = contactRepository;
            cca = ConfidentialClientApplicationBuilder
                    .Create(_configuration.GetSection("AzureAd")["ClientId"])
                    .WithClientSecret(_configuration.GetSection("AzureAd")["ClientSecret"])
                    .WithAuthority(new Uri($"https://login.microsoftonline.com/{_configuration.GetSection("AzureAd")["TenantId"]}"))
                    .Build();
        }

        // GET: ClientAccountController
        [Authorize]
        [Authorize(Roles = "GDXBillingService.FinancialOfficer, GDXBillingService.Owner, GDXBillingService.User")]
        public ActionResult Index(int ministryFilter, int numberFilter, string responsibilityFilter, string authorityFilter, string teamFilter, string keyword, string primaryContactFilter)
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
        public ActionResult Details(int id, bool isNew = false, bool isEdited = false)
        {
            ClientAccount? account = _clientAccountRepository.GetClientAccount(id);
            if (account == null)
                return NotFound();
            ViewData["isNew"] = isNew;
            ViewData["isEdited"] = isEdited;
            // check if user ought to be able to view this record
            if (!User.IsInRole("GDXBillingService.FinancialOfficer"))
            {

                if (!String.IsNullOrEmpty(User.GetDisplayName()))
                {
                    if (!IsUserAccountContact(account))
                    {
                        if (!String.IsNullOrEmpty(account.ExpenseAuthorityName) && !account.ExpenseAuthorityName.ToLower().Contains(GetUserLastName().ToLower()))
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
            //IEnumerable<Bill> charges = _billRepository.GetBillsByClientId(id);
            //IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();

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
        public async Task<IActionResult> Edit(ClientAccount model, int[] contactIds, int[] personIds, string[] displayNames, string[] contactTypes)
        {
            try
            {
                await ResolveContactUpdates(model.Id, contactIds, personIds, displayNames, contactTypes);
                await _clientAccountRepository.Update(model);

                return RedirectToAction("Details", new { model.Id, isEdited = true });
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
        public ActionResult Create()
        {
            IEnumerable<Ministry> ministries = _ministryRepository.GetAll();
            ClientCreateViewModel model = new ClientCreateViewModel();
            model.Organizations = ministries;

            return View(model);
        }

        [ServiceFilter(typeof(GroupAuthorizeActionFilter))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientCreateViewModel model)
        {
            try
            {
                if (!model.Account.OrganizationId.HasValue)
                    throw new Exception("Somehow and account with no organization Id was submitted");
                Ministry? org = _ministryRepository.GetById(model.Account.OrganizationId.Value);
                if (org == null)
                    throw new Exception($"No ministry or organization was found with an ID matching {model.Account.OrganizationId.Value}");
                // BusinessArea busArea = _businessAreaRepository.GetById(model.Account.OrganizationId.Value);
                string accountName = $"{org.Acronym} - {model.DivisionOrBranch}";
                model.Account.Name = accountName;
                ClientAccount account = model.Account;

                //lets continue collecting values for the contact fields on the account model for now. Just in case.
                account = PopulateContactFields(account, model.Approvers, "approver");
                account = PopulateContactFields(account, model.FinancialContacts, "financial");
                account = PopulateContactFields(account, model.PrimaryContacts.ToList(), "primary");
                account.ExpenseAuthorityName = model.ExpenseAuthorityContact;

                _logger.LogInformation($"Client Account with Id: {account.Id} is being added to DB");

                int accountId = _clientAccountRepository.AddClientAccount(account);

                await AddContactsToAccount(accountId, model.Approvers, "approver");
                await AddContactsToAccount(accountId, model.FinancialContacts, "financial");
                await AddContactsToAccount(accountId, model.PrimaryContacts.ToList(), "primary");
                List<string> expenseAsList = new List<string>();
                expenseAsList.Add(model.ExpenseAuthorityContact);
                await AddContactsToAccount(accountId, expenseAsList, "expense");



                //if(!String.IsNullOrEmpty(account.ExpenseAuthorityName))
                //{
                //    var expenseAuthority = await _graphApiService.GetUsersByDisplayName(account.ExpenseAuthorityName, cca);
                //    if (expenseAuthority is not null)
                //    {
                //        var eaId = expenseAuthority?.Value?.FirstOrDefault()?.Id;
                //        var eaEmail = (await _graphApiService.Me(eaId, cca)).UserPrincipalName;
                //        var eaDisplayName = (await _graphApiService.Me(eaId, cca)).DisplayName;
                //        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

                //        await _emailService.SendEmail(
                //            eaEmail,
                //            eaDisplayName,
                //            "Please Review: New GDX Service Billing Account Information",
                //            $@"
                //            Hello Andre Lashley,

                //            We hope this message finds you well. We're writing to inform you that a new account has been created for you in the GDX Service Billing system, designed to enhance your access and features.

                //            To complete the setup of your account, please verify its creation. We prioritize your security and do not include direct links in our emails. You can safely access the GDX Service Billing portal through our official website or your internal systems.

                //            If this account was not requested by you or if you believe you have received this email by mistake, please get in touch with our support team at [Support Contact Information] for immediate assistance.

                //            We appreciate your attention to this matter. Should you have any questions or require further assistance, do not hesitate to contact us.

                //            Warm regards,

                //            GDX Service Billing Team"
                //        );
                //    }
                //}

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

            return RedirectToAction("details", new { model.Account.Id, isNew = true });
        }

        private async Task AddContactsToAccount(int accountId, List<string> contacts, string contactType, Models.Contact? contactEntry = null)
        {
            try
            {
                foreach (string contact in contacts)
                { //Todo: handle the contact not being found because the user put in something dumb.
                    if (string.IsNullOrEmpty(contact))
                        continue;
                    Models.Person? person = _peopleRepository.GetPersonByDisplayName(contact);
                    if (person == null) // add new Person to DB
                    { // This is a bit hacky. It'd be better to get the GraphUser stuff from the model, but I can't be bothered right now.
                        string term = contact.Substring(0, contact.IndexOf(' ', contact.IndexOf(' ') + 1));
                        int personId = await _peopleRepository.AddPersonByDisplayName(term);
                        person = _peopleRepository.GetPersonById(personId);
                        if (person == null)
                            throw new Exception($"failed to add person to database: {term}");
                    }
                    if (contactEntry == null)
                    {
                        contactEntry = new Models.Contact();
                        contactEntry.ContactType = contactType;
                        contactEntry.PersonId = person.Id;
                        contactEntry.ClientAccountId = accountId;
                        await _contactRepository.AddContact(contactEntry);
                    }
                    else
                    {
                        contactEntry.PersonId = person.Id;
                        _contactRepository.UpdateContact(contactEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }


        private async Task<ActionResult> ResolveContactUpdates(int accountId, int[] contactIds, int[] personIds, string[] displayNames, string[] contactTypes)
        {
            IEnumerable<Models.Contact> existingContacts = _contactRepository.GetContactsByAccountId(accountId).ToList();
            IEnumerable<Models.Contact> removedContacts = existingContacts.Where(x => !contactIds.Contains(x.Id));

            foreach (Models.Contact contact in removedContacts)
            {
                _contactRepository.DeleteContact(contact);
            }

            for (int i = 0; i < contactIds.Length; i++)
            {
                int id = contactIds[i];
                List<string> nameList = new List<string>(); // for reusing AddContactsToAccount.
                nameList.Add(displayNames[i]);
                if (!existingContacts.Any(c => c.Id == id) || (id == 0 && !string.IsNullOrEmpty(displayNames[i])))
                { // new contact, and we have something for that
                    await AddContactsToAccount(accountId, nameList, contactTypes[i]);
                }
                else
                {
                    Models.Contact existingContact = existingContacts.First(c => c.Id == id);
                    if (existingContact.PersonId != personIds[i])
                    {
                        if (personIds[i] == 0)
                            await AddContactsToAccount(accountId, nameList, contactTypes[i], existingContact);
                        else
                            existingContact.PersonId = personIds[i];
                    }
                }
            }

            return Ok();
        }

        //We'll get rid of this whole hoopla once we're rock solid in the new contact tracking scheme, where contacts are tracked by
        //foreign key to the Contacts table
        private ClientAccount PopulateContactFields(ClientAccount account, List<string> contacts, string contactType)
        {
            string contactsString = string.Empty;
            foreach (string name in contacts)
            {
                contactsString += name == contacts.Last() ? name : $"{name}, ";
            }
            switch(contactType)
            {
                case "approver":
                    account.Approver = contactsString;
                    break;
                case "financial":
                    account.FinancialContact = contactsString;
                    break;
            }
            
            return account;
        }
   
        public ActionResult Approve(int id)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Approve(int id, IFormCollection collection)
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


        [HttpGet]
        public async Task<ActionResult?> GetAccountContact(string displayName, string email)
        {
            try
            {
                Models.Person? person = _peopleRepository.GetPersonByDisplayName(displayName);
                if(person == null) // The long arm of BCS always gets its person!
                {
                    GraphUser user = await _graphApiService.GetUserByDisplayName(displayName, cca);
                    person = new Models.Person();
                    person.Name = $"{user.GivenName} {user.Surname}";
                    person.Mail = user.Mail;
                    user.DisplayName = user.DisplayName;
                   // await _peopleRepository.AddPerson(person);
                }
                else if (person != null && string.IsNullOrEmpty(person.Mail))
                {
                    GraphUser user = await _graphApiService.GetUserByDisplayName(displayName, cca);
                    person.Mail = user.Mail;
                    await _peopleRepository.Update(person);
                }

                return new JsonResult(person);
                
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;    
            }
        }

        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public async Task<IActionResult> SearchForContact(string term)
        {
            try
            {
                var queriedUsers = await _graphApiService.GetUsersByDisplayName(term, cca);

                List<string> contacts = new List<string>();
                if(queriedUsers.Value != null)
                {
                    foreach (var user in queriedUsers.Value)
                    {
                        if (IsValidDisplayName(user.DisplayName))
                        {
                            contacts.Add(user.DisplayName);
                        }
                    }
                }

                return new JsonResult(contacts);
            }
            catch (ServiceException svcex)
            {
                _logger.LogError("Graph API service exception: ");
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

        private bool IsValidDisplayName(string displayName)
        {
            // Define your regex pattern here
            string pattern = @"[A-Za-z\s,]+ [A-Z]{2,}:[A-Z]{2}";
            return Regex.IsMatch(displayName, pattern);
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
            if (!String.IsNullOrEmpty(keyword))
            {
                clients = clients.Where(x => (!String.IsNullOrEmpty(x.Name) && x.Name.ToLower().Contains(keyword.ToLower())) ||
                (!String.IsNullOrEmpty(x.ResponsibilityCentre) && x.ResponsibilityCentre.ToLower().Contains(keyword.ToLower()) ||
                (!String.IsNullOrEmpty(x.Project) && x.Project.ToLower().Contains(keyword.ToLower())) ||
                (!String.IsNullOrEmpty(x.ServicesEnabled) && x.ServicesEnabled.ToLower().Contains(keyword.ToLower())) ||
                (!String.IsNullOrEmpty(x.ExpenseAuthorityName) && x.ExpenseAuthorityName.ToLower().Contains(keyword.ToLower())))
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
                    if (account.OrganizationId.HasValue && account.OrganizationId.Value > 0)
                    {
                        row.organization = _ministryRepository.GetById(account.OrganizationId.Value).Title;
                    }
                    else
                        row.organization = "NO ORGANIZATION SET"; // no one should ever see this.
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
                        if(account.Bills != null && account.Bills.Any())
                        {
                            foreach (Bill charge in account.Bills)
                            {
                                charge.IsActive = false;
                                _billRepository.Update(charge);
                            }
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


        // Right now this just checks if the current user has a last name and first name that are both included in a contact string.
        // It could certainly be improved by having all contact entries match their Azure AD display name,
        // like "Alexander.Carmichael@gov.bc.ca, or by taking advantage of a "people table" identity lookup scheme.
        public bool IsUserAccountContact(ClientAccount account)
        {
            string? userName = User.GetDisplayName();
            string lastName = string.Empty;
            string firstName = string.Empty;
            if (!String.IsNullOrEmpty(userName)) //Firstname.Lastname@Gov.bc.ca
            {
                string[] nameElements = userName.Split('.');
                if (nameElements.Length > 1)
                {
                    lastName = nameElements[1].Substring(0, nameElements[1].IndexOf('@'));
                    lastName = lastName.Trim();
                    firstName = nameElements[0];
                }
            }
            if (!String.IsNullOrEmpty(lastName))
            {
                if (!String.IsNullOrEmpty(lastName) &&
                        (!String.IsNullOrEmpty(account.PrimaryContact) && 
                        (account.PrimaryContact.ToLower().Contains(lastName.ToLower()) && account.PrimaryContact.ToLower().Contains(firstName.ToLower()))) ||
                        (!String.IsNullOrEmpty(account.FinancialContact) && 
                        (account.FinancialContact.ToLower().Contains(lastName.ToLower()) && account.FinancialContact.ToLower().Contains(firstName.ToLower()))) ||
                        (!String.IsNullOrEmpty(account.Approver) && 
                        (account.Approver.ToLower().Contains(lastName.ToLower()) && account.Approver.ToLower().Contains(firstName.ToLower())))
                    )
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
        public short? casClientNumber;
        public string? organization;
        public string aggregateGLCode;
        public string? servicesEnabled;
        public string primaryContact;
        public string financialContact;
        public string approver;
        public string? expenseAuthority;
        public bool approved;
        public bool active;
        public string notes;

        public ClientInfoRowEntry(ClientAccount account)
        {
            clientId = account.Id;
            clientName = !String.IsNullOrEmpty(account.Name)? account.Name : String.Empty;
            casClientNumber = account.ClientNumber;
            primaryContact = !String.IsNullOrEmpty(account.PrimaryContact)? account.PrimaryContact : String.Empty;
            financialContact = !String.IsNullOrEmpty(account.FinancialContact)? account.FinancialContact : String.Empty;
            approver = !String.IsNullOrEmpty(account.Approver)? account.Approver : String.Empty;
            approved = account.IsApprovedByEA;
            active = account.IsActive;
            aggregateGLCode = account.AggregatedGLCode;
            notes = !String.IsNullOrEmpty(account.Notes)? account.Notes : String.Empty;
        }
    }
}
