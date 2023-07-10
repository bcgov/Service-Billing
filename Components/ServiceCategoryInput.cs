using Microsoft.AspNetCore.Mvc;
using Service_Billing.Models;

namespace Service_Billing.Components
{
    public class ServiceCategoryInput : ViewComponent
    {
        public IViewComponentResult Invoke(IEnumerable<ServiceCategory> categories, int id)
        {
            ViewData["id"] = id;
            return View(categories);
        }
    }
}
