using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Billing.Models
{
    public class Contact
    {
        [Key] 
        public int Id { get; set; }

        public int PersonId { get; set; }
        [Required]
        [ForeignKey("PersonId")]
        public virtual Person? Person { get; set; }

        public int ClientAccountId { get; set; }
        [Required]
        [ForeignKey("ClientAccountId")]
        public ClientAccount? Account;
        public string ContactType { get; set; } = String.Empty; // ('primary', 'approver', 'financial', 'expense')

        public Contact(int clientId, string contactType)
        {
            ClientAccountId = clientId;
            ContactType = contactType;
        }
        public Contact() { }
    }
}
