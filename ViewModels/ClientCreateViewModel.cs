using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Service_Billing.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Billing.ViewModels
{
    public class ClientCreateViewModel
    {
        public int Id { get; set; }
        public ClientAccount Account { get; set; }
        public string? MinistryAcronym { get; set; }

        [Display (Prompt = "Enter a division or branch name")]
        public string? DivisionOrBranch { get; set; }
      //  public string? ApproverName { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem>? Contacts { get; set; }
        [NotMapped]
        public IEnumerable<Ministry>? Organizations { get; set; }

        //[BindRequired]
        //[Display(Prompt = "Start typing in your contact's last name")]
        //public string PrimaryContactInput { get; set; }
        //public string PrimaryContactInputSecond { get; set; }

        //[BindRequired]
        //[Display(Prompt = "Start typing in your contact's last name")]
        //public string ExpenseAuthorityInput { get; set; }

        [BindRequired]
        public List<string> Approvers { get; set; }

        [BindRequired]
        public List<string> FinancialContacts { get; set; }


        public ClientCreateViewModel()
        {
            Account = new ClientAccount();
            Approvers = new List<string>();
            FinancialContacts = new List<string>();
            Approvers.Add(String.Empty);
            FinancialContacts.Add(string.Empty);
            
        }
    }
}
