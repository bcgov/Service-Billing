using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;

namespace Service_Billing.Models
{
    /*6.	Enter data:

•	URL or IDIR: depending on the service, enter the site or the user ID of the affected person or site.
•	Billing Cycle: auto-populates for first day of quarterly cycle, e.g.: 1/1/2018 = Fiscal Year 17/18 Quarter 4. In almost all cases, you will not have to change the value.
•	Client Account: The Client Account (billing account) provided by Service Desk in ticket via triage process.
•	Service Category: referencing the Rates list, need new link -  determining the price per quantity

•	Prorated: (use “Yes and round quantity to Integer”)
o	No: uses quantity field to set number of months per cycle (do not use)
o	Yes: for the portion of the month the service was used, bill for that fraction of the monthly cost
o	Yes, and round quantity to Integer (default): rounds to a whole number for the quantity, e.g. Apr 5 to June 30 = quantity of 2 months
•	Ticket Number and Requester Name: Reference your service desk ticket which created this fee and person who requested it. This helps clients review their charges via the ticketing system.
7.	Leave empty:
•	Service Request Date: This is used for one-time fees.
•	Amount: Only use this field to override the Service Category/Rates list/Quantity calculation


    6.	Enter data:

•	Service Request Date: The date the work is done (should be today’s date)
•	Billing Cycle: auto-populates for first day of quarterly cycle, e.g.: 1/1/2018 = Fiscal Year 17/18 Quarter 4. In almost all cases you will not have to change the value.
•	Account: The Client Account (billing account) provided by Service Desk in ticket via triage process
•	Ticket Number and Requester Name: Reference your service desk ticket which created this fee and person who requested it. This helps clients review their charges via the ticketing system.
•	Service Category: referencing the Rates list – need link, determining the price per quantity
•	Quantity: per Each Hour or Month
7.	Leave empty:
•	Amount: 
*/
    public class BillEntryViewModel
    {
        public BillEntryViewModel() { }
        private string affectedSiteOrPerson = ""; // "URL or IDIR: depending on the service, enter the site or the user ID of the affected person or site. "
        public DateTime? RequestDate { get; set; }  // one-time fees only
        public DateTime BillingCycle { get; set; }// Not sure of format of this. some custom object class might be better "Billing Cycle: auto-populates for first day of quarterly cycle, e.g.: 1/1/2018 = Fiscal Year 17/18 Quarter 4"
        public int Account { get; set; } // see https://citz.sp.gov.bc.ca/sites/GDX/billing/Lists/Organizations/AllItems.aspx
        public int? TicketNumber { get; set; }
        private string requesterName = ""; //We probably need a proper contact model, not just a string.
        public int ServiceCategory { get; set; } // assuming service categories are a key/value pair fetched form DB. We'll see.
        public int Quantity { get; set; }// not sure yet what the idea is here.
        public double? Ammount { get; set; } // Only use this field to override the Service Category/Rates list/Quantity calculation
        public DateTime? ServiceStartDate { set; get; } // only used for Fixed Consumptions / monthly services.
        public DateTime? ServiceEndDate { get; set; }  // only used for Fixed Consumptions / monthly services.
        public bool IsOneTime { get; set; }

        public string AffectedSiteOrPerson { 
            get { return affectedSiteOrPerson; } 
            set {  affectedSiteOrPerson = value; } 
        }
        public string RequesterName { 
            get {  return requesterName; } 
            set {  requesterName = value; } 
        }

    }
}
