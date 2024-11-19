using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Billing.Models
{
    public class Contact
    {
        [Key] public int Id { get; set; }

        public int PersonId { get; set; }
        [Required]
        [ForeignKey("PersonId")]
        public Person Person { get; set; }

        public int AccountId { get; set; }
        public string ContactType { get; set; } = String.Empty; // ('primary', 'approver', 'financial', 'expense')
    }
}
