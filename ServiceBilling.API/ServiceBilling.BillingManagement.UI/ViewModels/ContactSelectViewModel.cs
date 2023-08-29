using Microsoft.AspNetCore.Mvc.Rendering;

namespace ServiceBilling.BillingManagement.UI.ViewModels
{
    public class ContactSelectViewModel
    {
        public string? elementId { get; set; }
        public IEnumerable<SelectListItem> Contacts { get; set; }

        public ContactSelectViewModel(string? id, IEnumerable<SelectListItem> list)
        {
            elementId = id;
            Contacts = list;
        }
    }
}
