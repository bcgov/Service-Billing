using Microsoft.AspNetCore.Mvc;

namespace ServiceBilling.BillingManagement.UI.Controllers
{
    public class ServiceChargesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
