using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Billing.Models
{
    public class ClientTeam
    {
        [Key]
        [Column("Team_Id")]
        public int teamId { get; set; }
        [Column("Client_Team_Name")]
        public string? teamName { get; set; }
        [Column("Financial_Contacts")]
        public string? financialContacts { get; set; }
        [Column("Primary_Contact")]
        public string? primaryContact { get; set; }
        [Column("Approvers")]
        public string? approvers { get; set; }
        [Column("Created")]
        public DateTime? dateCreated { get; set; }

    }
}
