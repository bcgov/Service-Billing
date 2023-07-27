using Microsoft.AspNetCore.Mvc.Rendering;

namespace Service_Billing.ViewModels
{
    public class ContactSelectViewModel
    {
        public string elementId { get; set; }   
        public string contactType { get; set; }
        public IEnumerable<SelectListItem> Contacts { get; set; }

        public ContactSelectViewModel(string? id, IEnumerable<SelectListItem> list, string? contactType)
        {
            elementId = id;
            Contacts = list;
            this.contactType = contactType;
        }
    }
}
