﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Service_Billing.Models;
using Service_Billing.ViewModels;

namespace Service_Billing.Controllers
{
    public class ClientAccountController : Controller
    {
        private readonly IClientAccountRepository _clientAccountRepository;
        private readonly IClientTeamRepository _clientTeamRepository;
        private readonly IMinistryRepository _ministryRepository;

        public ClientAccountController(IClientAccountRepository clientAccountRepository, IClientTeamRepository clientTeamRepository, IMinistryRepository ministryRepository)
        {
            _clientAccountRepository = clientAccountRepository;
            _clientTeamRepository = clientTeamRepository;
            _ministryRepository = ministryRepository;
        }
        // GET: ClientAccountController
        public ActionResult Index()
        {
            IEnumerable<ClientAccount> clients = _clientAccountRepository.GetAll();

            return View(new ClientAccountViewModel(clients));
        }

        // GET: ClientAccountController/Details/5
        public ActionResult Details(int id)
        {
            ClientAccount? account = _clientAccountRepository.GetClientAccount(id);
            if (account == null)
                return NotFound();
            ClientTeam? team = _clientTeamRepository.GetTeamById(account.TeamId);
            ViewData["clientTeam"] = team != null? team : "";
            return View(account);
        }

        // GET: ClientAccountController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ClientAccountController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
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
            ClientAccount? accountToUpdate =  _clientAccountRepository.GetClientAccount(id);
            if(accountToUpdate == null)
            {
                return NotFound();
            }
            if(await TryUpdateModelAsync<ClientAccount>(accountToUpdate, "", 
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

            return Details(id);
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
                string accountName = $"{model.MinistryAcronym} - {model.DivisionOrBranch}";
                model.Account.Name = accountName;
                //if validation passes, etc...
                ClientTeam team = model.Team;
                int teamId = _clientTeamRepository.Add(team);

                ClientAccount account = model.Account;
                account.TeamId = teamId;
            }
            catch(DbUpdateException ) 
            {
                return View(new ClientIntakeViewModel());
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
