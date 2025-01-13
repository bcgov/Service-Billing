using Microsoft.AspNetCore.Mvc;
using Service_Billing.Models;

namespace Service_Billing.Components
{
    public class ContactInput : ViewComponent
    {
        public IViewComponentResult Invoke(Contact? contact, string id)
        {
            return View(contact);
        }
    }
}
