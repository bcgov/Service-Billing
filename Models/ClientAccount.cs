using Microsoft.AspNetCore.Mvc.ModelBinding;
using NuGet.Configuration;
using Service_Billing.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Billing.Models
{
    // see https://apps.itsm.gov.bc.ca/confluence/display/GDXSB/Account+Setup for client documentation
    public class ClientAccount
    {
        [Key]
        public int Id { get; set; }

        /*	<sector/group> - <ministry acronym> - <sub group> - <full org/branch name> - <service>; e.g.: ECON – JTT – DataBC – WordPress; CITZ – OCIO – ISB. 
o	Note: not all naming convention components are required. The naming convention is used to easily group and sort Client Accounts along organizational lines.
*/
        [Required(ErrorMessage = "Add a client name")]
        [Display(Name = "Name")]
        [ClientNameValidation]
        public string? Name { get; set; }

        [BindRequired]
        //"must be unique, except for secondary accounts only used for alternative financial coding."
        [Display(Name = "CAS Client Number")]
        public Int16? ClientNumber { get; set; } // three digit billing code

        //5-digits or combination of digits and letters
        [BindRequired]
        [StringLength(5)]
        [MinLength(5)]
        [MaxLength(5)]
        [Display(Name = "Responsibility Center")]
        public string? ResponsibilityCentre { get; set; }

        [BindRequired]
        [Display(Name = "Service Line")]
        public int? ServiceLine { get; set; }

        [BindRequired]
        public Int16? STOB { get; set; }

        [Required(ErrorMessage = "A Project Code must be provided")]
        [StringLength(7)]
        [MinLength(7)]
        [Display(Name = "Project")]
        public string? Project { get; set; }


        [Required(ErrorMessage = "Please include this contact")]
        [Display(Name = "Expense Authority")]
        public string? ExpenseAuthorityName { get; set; }

        [Display(Name = "Services Enabled")]
        public string? ServicesEnabled { get; set; } // string so we can have a list of id's, like "3, 6, 420"

        [Display(Name = "EA Approved")]
        public bool IsApprovedByEA { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public string? Notes { get; set; }

        public virtual ICollection<Bill>? Bills { get; set; }
        
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
        [Display(Name = "Financial Contacts")]
        public string? FinancialContact { get; set; }

        [NotMapped]
        public string AggregatedGLCode
        {
            get
            {
                return $"{ClientNumber}.{ResponsibilityCentre}.{ServiceLine}.{STOB}.{Project}";
            }
        }

        [BindRequired]
        public int? OrganizationId { get; set; } = 0;//for ministry/organization tracking
    }
}
/* some validation concerns:
 * Client= 3 digits
Responsibility Centre= 5 digits
Service Line= 5 digits
STOB= 4 digits
Project= 7 digits
*/