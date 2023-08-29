using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ServiceBilling.BillingManagement.UI.Models
{
    public class Charge
    {
        [Key]
        public int Id { get; set; }
        public int ClientAccountId { get; set; }

        [Display(Name = "Client Name")]
        public string? ClientName { get; set; }

        [Display(Name = "Title")]
        public string? Title { get; set; }

        [Display(Name = "URL or IDIR")]
        public string? IdirOrUrl { get; set; }

        public Guid ServiceCategoryId { get; set; }

        [Display(Name = "Amount")]
        public decimal? Amount { get; set; }

        [Display(Name = "Fiscal Period")]
        public string? FiscalPeriod { get; set; }
        //  public decimal Unit_Price; //comes from service category lookup
        public decimal? Quantity { get; set; }

        [Column("ticketNumberAndRequesterName")]
        [Display(Name = "Ticket Number")]
        public string? TicketNumberAndRequester { get; set; }

        public DateTime? DateModified { get; set; }
        public DateTime? DateCreated { get; set; }
        public string? BillingCycle { get; set; }
        public string? CreatedBy { get; set; }

        public Charge()
        {
            this.Quantity = 1;
       
            this.DateCreated = DateTime.UtcNow;
        }
    }
}
