﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Graph;
using Service_Billing.Models;
using Service_Billing.Services;
using Service_Billing.ViewModels;

namespace Service_Billing.Components
{
    public class ContactLookup : ViewComponent
    {
        private IContactLookupService _contactLookupService;
        private string _contactName {  get; set; }

        public async Task<IViewComponentResult> InvokeAsync(string? elementId, List<SelectListItem> contactList )
        {
            ContactSelectViewModel contactSelectViewModel = new ContactSelectViewModel(elementId, contactList);
            return View(contactSelectViewModel);
        }

    }
}
