using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Billing.Models
{
    public class ClientTeam
    {
        [Key]
        public int Id { get; set; }

        /* must follow the same naming convention as the primary Client Account name which it is associated with and 
         * ends with the word “Team”, e.g.: ENV – BC Parks Volunteers – WordPress Team. */
        [Display(Name ="Team Name")]
        public string? Name { get; set; }

        /* This is the Primary Contact for the Client Account.  Normally there is only one.  
           This role can authorize billable service requests and changes to client account details including new services, 
           client account team membership, financial coding changes and SharePoint site access for their team.
        */
     //   [Required(ErrorMessage = "Please include this contact")]
        [Display(Name = "Primary Contact")]
        public string? PrimaryContact { get; set; }

        /* This role can authorize billable service requests and changes to client account details including new services, 
         * client account team membership, financial coding changes and SharePoint site access for their team.  */
     //   [Required(ErrorMessage = "Please include this contact")]
        public string? Approver { get; set; }

        /* This role is not normally involved with service request approvals, though an exception can be made if the primary, 
         * or approvers are not available. The role can provide updated billing information.  
         * For quarterly billing, this role is a contact. */
      //  [Required(ErrorMessage = "Please include this contact")]
        [Display(Name ="Financial Contacts")]
        public string? FinancialContact { get; set; }
        //[Display(Name = "Date Created")]
        //public DateTime? Created { get; set; }


        
    }
}
