using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Billing.Models
{
    public class Bill
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("ClientAccount")]
        public int ClientAccountId { get; set; }
        public ClientAccount ClientAccount { get; set; }

        [ForeignKey("ServiceCategory")]
        public int ServiceCategoryId { get; set; }
        public ServiceCategory ServiceCategory { get; set; }
  
        [Display(Name = "Title")]
        public string? Title { get; set; }

        [Display(Name = "URL or IDIR")]
        public string? IdirOrUrl { get; set; }

        

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

        public string? AggregateGLCode { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<FiscalPeriod>? fiscalPeriods { get; set; }


        public Bill()
        {
            this.Quantity = 1;
            this.DateCreated = DateTime.UtcNow;
            this.StartDate = DateTime.UtcNow;
            this.IsActive = true;
            fiscalPeriods = new Collection<FiscalPeriod>();
        }
    }
}
