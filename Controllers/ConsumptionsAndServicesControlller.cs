using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Service_Billing.Controllers
{
    public class ConsumptionsAndServicesControlller : Controller
    {
        // GET: ConsumptionsAndServicesControlller
        public ActionResult Index()
        {
            return View();
        }

        // GET: ConsumptionsAndServicesControlller/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ConsumptionsAndServicesControlller/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ConsumptionsAndServicesControlller/Create
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

        // GET: ConsumptionsAndServicesControlller/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ConsumptionsAndServicesControlller/Edit/5
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

        // GET: ConsumptionsAndServicesControlller/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ConsumptionsAndServicesControlller/Delete/5
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
