using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Service_Billing.Controllers
{
    /*"As GDX staff responsible for service billing, I need to manage all Service Category types, rates and associated details, so 
     * that both GDX staff and Clients have visibility of current services & cost for services, licences and fees."*/

    public class ServiceCategoryController : Controller
    {
        // GET: ServiceCategoryController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ServiceCategoryController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ServiceCategoryController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ServiceCategoryController/Create
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

        // GET: ServiceCategoryController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ServiceCategoryController/Edit/5
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

        // GET: ServiceCategoryController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ServiceCategoryController/Delete/5
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
