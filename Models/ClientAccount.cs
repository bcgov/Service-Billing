using Microsoft.AspNetCore.Mvc.ModelBinding;
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

        /*	<sector/group> - <ministry acronym> - <sub group> - <full org/branch name> - <service>; e.g.: ECON – JTT – DataBC – WordPress; CITZ – OCIO – ISB. 
o	Note: not all naming convention components are required. The naming convention is used to easily group and sort Client Accounts along organizational lines.
*/
        [Required(ErrorMessage = "Add a client name")]
        [Display(Name = "Name")]
        public string? Name { get; set; }

        [BindRequired]
        //"must be unique, except for secondary accounts only used for alternative financial coding."
        [Display(Name = "Client Account Number", Prompt = "00")]
        public Int16? ClientNumber { get; set; } // three digit billing code

        //5-digits or combination of digits and letters
        [BindRequired]
        [RegularExpression(@"^.{5,5}$", ErrorMessage = "Please provide the five-digit responsibility centre code from the Corporate Accounting System.")]
        [Display(Name = "Responsibility Centre", Prompt = "00000")]
        public string? ResponsibilityCentre { get; set; }

        [BindRequired]
        [RegularExpression(@"^.{5,5}$", ErrorMessage = "Please provide the five-digit service line code from the Corporate Accounting System.")]
        [Display(Name = "Service Line", Prompt = "00000")]
        public int? ServiceLine { get; set; }

        [BindRequired]
        [Display (Prompt = "0000")]
        [RegularExpression(@"^.{4,4}$", ErrorMessage = "Please provide the ministry Standard Object of Expenditure (STOB) number.")]
        public Int16? STOB { get; set; }

        [Required(ErrorMessage = "A Project Code must be provided")]
        [RegularExpression(@"^.{7,7}$", ErrorMessage = "Please provide a ministry project code.")]
        [Display(Name = "Project", Prompt = "1500000")]
        public string? Project { get; set; }

        [Display(Name = "Expense Authority", Prompt = "Start typing in your contact's last name")]
        [Required(ErrorMessage = "Please provide the name of the ministry expense authority")]
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
        [Display(Name = "Primary Contact", Prompt = "Start typing in your contact's last name")]
        public string? PrimaryContact { get; set; }

        /* This role can authorize billable service requests and changes to client account details including new services, 
         * client account team membership, financial coding changes and SharePoint site access for their team.  */
        [Display (Prompt = "Start typing in your contact's last name")]
        public string? Approver { get; set; }

        /* This role is not normally involved with service request approvals, though an exception can be made if the primary, 
         * or approvers are not available. The role can provide updated billing information.  
         * For quarterly billing, this role is a contact. */
        //  [Required(ErrorMessage = "Please include this contact")]
        [Display(Name = "Financial Contacts", Prompt = "Start typing in your contact's last name")]
        public string? FinancialContact { get; set; }

        [NotMapped]
        public string AggregatedGLCode
        {
            get
            {
                return $"{ClientNumber}.{ResponsibilityCentre}.{ServiceLine}.{STOB}.{Project}";
            }
        }
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