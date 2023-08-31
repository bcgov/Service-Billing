using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Service_Billing.ViewModels;

namespace Service_Billing.Components
{
    public class ContactLookup : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string? elementId, List<SelectListItem> contactList )
        {
            ContactSelectViewModel contactSelectViewModel = new ContactSelectViewModel(elementId, contactList);
            return View(contactSelectViewModel);
        }

    }
}
