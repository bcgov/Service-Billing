using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service_Billing.Models;

namespace Service_Billing.Controllers
{
    public class BillingControlller : Controller
    {
        // GET: BillingControlller
        [Route("/Billing")]
        public IActionResult Index()
        {
            BillEntryViewModel model = new BillEntryViewModel();
            // Todo: logic for deciding if it's a one-time or ongoing consupmtion goes here
            model.IsOneTime = false;
            return View("../Billing/Index", model);
        }

        // GET: BillingControlller/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: BillingControlller/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: BillingControlller/Create
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

        // GET: BillingControlller/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: BillingControlller/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
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

        // GET: BillingControlller/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: BillingControlller/Delete/5
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
    }
}
