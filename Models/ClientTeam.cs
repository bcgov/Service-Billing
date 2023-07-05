using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Billing.Models
{
    public class ClientTeam
    {
        [Key]
        public int Id { get; set; }

        [Display(Name ="Team Name")]
        public string? Name { get; set; }

        [Display(Name ="Financial Contacts")]
        public string? FinancialContacts { get; set; }

        [Display(Name ="Primary Contact")]
        public string? PrimaryContact { get; set; }

        public string? Approvers { get; set; }

        [Display(Name ="Date Created")]
        public DateTime? Created { get; set; }

    }
}
