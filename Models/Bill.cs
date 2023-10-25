using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Billing.Models
{
    public class Bill
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

        public int? ServiceCategoryId { get; set; }

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
        public DateTime? EndDate { get; set; }
        public DateTime? StartDate { get; set; }
        public string? BillingCycle { get; set; }
        public string? CreatedBy { get; set; }

        public Bill()
        {
            this.Quantity = 1;
            this.ServiceCategoryId = -1;
            this.DateCreated = DateTime.UtcNow;
        }
    }
}
