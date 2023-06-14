﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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

            ViewBag.ServiceCategories = categories.ToList();
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

        public ActionResult Edit(int id)
        {
            Bill? bill = _billRepository.GetBill(id);
            List<ServiceCategory> categories = _categoryRepository.GetAll().Where(c => c.isActive == true).ToList();
            ViewData["ServiceCategories"] = categories;
            if (bill == null)
                return NotFound();
            bill.dateModified = DateTime.Now;
            return View(bill);
        }

        // POST: ClientAccountController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormCollection collection)
        {
            Bill? billToUpdate = _billRepository.GetBill(id);
            if (billToUpdate == null)
            {
                return NotFound();
            }
            if (await TryUpdateModelAsync<Bill>(billToUpdate, "",
                b => b.clientAccountId,
                b => b.clientName,
                b => b.title,
                b => b.idirOrUrl,
                b => b.serviceCategoryId,
                b => b.amount, // should be calculated based on service?
                b => b.quantity,
                b => b.ticketNumberAndRequester,
                b => b.dateModified,
                b => b.createdBy
                ))
            {
                try
                {
                    await _billRepository.SaveChangesAsync();
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
    }
}
