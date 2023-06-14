using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Billing.Models
{
    public class ClientTeam
    {
        [Key]
        [Column("Team_Id")]
        public byte teamId { get; set; }

        [Column("Client_Team_Name")]
        [Display(Name ="Team Name")]
        public string? teamName { get; set; }

        [Column("Financial_Contacts")]
        [Display(Name ="Financial Contacts")]
        public string? financialContacts { get; set; }

        [Column("Primary_Contact")]
        [Display(Name ="Primary Contact")]
        public string? primaryContact { get; set; }
        [Column("Approvers")]
        public string? approvers { get; set; }
        [Column("Created")]
        [Display(Name ="Date Created")]
        public DateTime? dateCreated { get; set; }

    }
}
