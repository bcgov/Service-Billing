using NuGet.Configuration;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Billing.Models
{
    // see https://apps.itsm.gov.bc.ca/confluence/display/GDXSB/Account+Setup for client documentation
    public class ClientAccount
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Name")]
        public string? Name { get; set; }

        [Display(Name = "Client Number")]
        public Int16? ClientNumber { get; set; } // three digit billing code

        [Display(Name = "Responsibility Center")]
        public string? ResponsibilityCentre { get; set; }

        [Display(Name = "Service Line")]
        public int? ServiceLine { get; set; }

        public Int16? STOB { get; set; }

        [Display(Name = "Project")]
        public string? Project { get; set; }

        [Display(Name = "Expense Authority")]
        public string? ExpenseAuthorityName { get; set; }

        [Display(Name = "Services Enabled")]
        public string? ServicesEnabled { get; set; } // string so we can have a list of id's, like "3, 6, 420"

        [Display(Name = "Client Team")]
        public string? ClientTeam { get; set; }

        [Column("Created")]
        public DateTime? DateCreated { get; set; }

        public int? TeamId { get; set; }
  
        public bool IsApprovedByEA { get; set; }
    }
}
/* some validation concerns:
 * Client= 3 digits
Responsibility Centre= 5 digits
Service Line= 5 digits
STOB= 4 digits
Project= 7 digits
*/