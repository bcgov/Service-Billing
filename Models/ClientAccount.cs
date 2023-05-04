using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;

namespace Service_Billing.Models
{
    // see https://apps.itsm.gov.bc.ca/confluence/display/GDXSB/Account+Setup for client documentation
    public class ClientAccount: PageModel
    {
        public void OnGet()
        {
            ViewData["someString"] = "hello from model";
        }

    }
}
