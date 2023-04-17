using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Service_Billing.Controllers
{
    public class ClientAccountController : Controller
    {
        // GET: ClientAccountController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ClientAccountController/Details/5
        public ActionResult Details(int id)
        {
            return View();
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
            return View();
        }

        // POST: ClientAccountController/Edit/5
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
    }
}
