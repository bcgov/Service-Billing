using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceBilling.BillingManagement.UI.ViewModels;

namespace ServiceBilling.BillingManagement.UI.Components
{
    public class ContactLookup : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string? elementId, List<SelectListItem> contactList)
        {
            ContactSelectViewModel contactSelectViewModel = new ContactSelectViewModel(elementId, contactList);
            return View(contactSelectViewModel);
        }
    }
}
