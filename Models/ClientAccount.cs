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
        public int AccountId { get; set; }
        [Column("Client_Account_Name")]
        public string? ClientName { get; set; }
        public int Client { get; set; }
        [Column("Responsibility_Centre")]
        public string? ResponsibilityCentre { get; set; }
        [Column("Service_Line")]
        public int ServiceLine { get; set; }
        public int STOB { get; set; }
        public int Project { get; set; }
        [Column("Expense_Authority_Name")]
        public string? Expense_Authority_Name { get; set; }
        [Column("Services_Enabled")]
        public int[]? ServicesEnabled { get; set; }
        [Column("Client_Team")]
        public int ClientTeam { get; set; }
        [Column("Created")]
        public DateOnly dateCreated { get; set; }
    }
}
