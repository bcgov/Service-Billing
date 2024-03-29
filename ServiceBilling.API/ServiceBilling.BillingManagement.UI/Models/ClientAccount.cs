﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace ServiceBilling.BillingManagement.UI.Models
{
    // see https://apps.itsm.gov.bc.ca/confluence/display/GDXSB/Account+Setup for client documentation
    public class ClientAccount
    {
        [Key]
        public int Id { get; set; }

        /*	<sector/group> - <ministry acronym> - <sub group> - <full org/branch name> - <service>; e.g.: ECON – JTT – DataBC – WordPress; CITZ – OCIO – ISB. 
o	Note: not all naming convention components are required. The naming convention is used to easily group and sort Client Accounts along organizational lines.
*/
        [Display(Name = "Name")]
        public string? Name { get; set; }

        [BindRequired]
        [StringLength(3)]
        [MinLength(3)]
        //"must be unique, except for secondary accounts only used for alternative financial coding."
        [Display(Name = "Client Number")]
        public Int16? ClientNumber { get; set; } // three digit billing code

        //5-digits or combination of digits and letters
        [BindRequired]
        [StringLength(5)]
        [MinLength(5)]
        [Display(Name = "Responsibility Center")]
        public string? ResponsibilityCentre { get; set; }

        [BindRequired]
        [StringLength(5)]
        [MinLength(5)]
        [Display(Name = "Service Line")]
        public int? ServiceLine { get; set; }

        [BindRequired]
        [StringLength(4)]
        [MinLength(4)]
        public Int16? STOB { get; set; }

        [BindRequired]
        [StringLength(5)]
        [MinLength(5)]
        [Display(Name = "Project")]
        public string? Project { get; set; }


        [BindRequired]
        [Display(Name = "Expense Authority")]
        public string? ExpenseAuthorityName { get; set; }

        [Display(Name = "Services Enabled")]
        public string? ServicesEnabled { get; set; } // string so we can have a list of id's, like "3, 6, 420"

        [Display(Name = "Client Team")]
        public string? ClientTeam { get; set; }

        public int? TeamId { get; set; }

        public bool IsApprovedByEA { get; set; } = false;
    }
}
