using NuGet.Configuration;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Billing.Models
{
    // see https://apps.itsm.gov.bc.ca/confluence/display/GDXSB/Account+Setup for client documentation
    public class ClientAccount
    {
        [Key]
        [Column("Client_Account_Number")]
        public Int16 accountId { get; set; }
        [Column("Client_Account_Name")]
        public string? clientName { get; set; }
        public Int16? client { get; set; }
        [Column("Responsibility_Centre")]
        public string? responsibilityCentre { get; set; }
        [Column("Service_Line")]
        public int? serviceLine { get; set; }
        public Int16? STOB { get; set; }
        public string? project { get; set; }
        [Column("Expense_Authority_Name")]
        public string? expense_Authority_Name { get; set; }
        [Column("Services_Enabled")]
        public string? servicesEnabled { get; set; } // string so we can have a list of id's, like "3, 6, 420"
        //[Column("Client_Team")]
        //public string? clientTeam { get; set; }
        [Column("Created")]
        public DateTime dateCreated { get; set; }
    }
}
/* some validation concerns:
 * Client= 3 digits
Responsibility Centre= 5 digits
Service Line= 5 digits
STOB= 4 digits
Project= 7 digits
*/