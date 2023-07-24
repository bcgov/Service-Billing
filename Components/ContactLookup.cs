using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Service_Billing.Models;
using Service_Billing.Services;

namespace Service_Billing.Components
{
    public class ContactLookup : ViewComponent
    {
        private IContactLookupService _contactLookupService;

        public ContactLookup(IContactLookupService contactLookupService)
        {
            _contactLookupService = contactLookupService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string query)
        {
            var contacts = await _contactLookupService.LookupAsync(query);
            return View(contacts);
        }
        //public IViewComponentResult Invoke()
        //{
        //    return View();
        //}

    }
}
