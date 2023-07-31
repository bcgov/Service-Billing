using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Graph;
using Service_Billing.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Billing.ViewModels
{
    public class ClientIntakeViewModel
    {
        public int Id { get; set; }
        public ClientTeam Team { get; set; }
        public ClientAccount Account { get; set; }
        public string? MinistryAcronym { get; set; }
        public string? DivisionOrBranch { get; set; }
      //  public string? ApproverName { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem> Contacts { get; set; }

        public ClientIntakeViewModel()
        {
            Team = new ClientTeam();
            Account = new ClientAccount();
    
        }
    }
}
