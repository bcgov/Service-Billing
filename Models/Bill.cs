﻿using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Billing.Models
{
    public class Bill
    {
        [Key]
        public int Id { get; set; }
        public int ClientAccountId { get; set; }
        [ForeignKey("ClientAccountId")]
        public virtual ClientAccount ClientAccount { get; set; }

        [ForeignKey("ServiceCategory")]
        [Required(ErrorMessage ="Add a service category from the drop down list.")]
        public int ServiceCategoryId { get; set; }
        public virtual ServiceCategory ServiceCategory { get; set; }
  
        [Display(Name = "Title")]
        public string? Title { get; set; }

        [Display(Name = "URL or IDIR")]
        public string? IdirOrUrl { get; set; }

        [Display(Name = "Amount")]
        public decimal? Amount { get; set; }

        public decimal? Quantity { get; set; }

        [Column("ticketNumberAndRequesterName")]
        [Display(Name = "Ticket Number")]
        public string? TicketNumberAndRequester { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTimeOffset? DateModified { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTimeOffset? DateCreated { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTimeOffset? EndDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTimeOffset? StartDate { get; set; }
        public string? BillingCycle { get; set; }
        public string? CreatedBy { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
        public virtual ICollection<FiscalHistory>? previousFiscalRecords { get; set; }
        public int CurrentFiscalPeriodId { get; set; } // "LastActiveFiscalPeriodID" would have been a better name

        [ForeignKey("CurrentFiscalPeriodId")]
        public virtual FiscalPeriod MostRecentActiveFiscalPeriod { get; set; }



        public Bill()
        {
            this.Quantity = 1;
            this.DateCreated = DateTimeOffset.UtcNow;
            this.StartDate = DateTimeOffset.UtcNow;
            this.IsActive = true;
            previousFiscalRecords = new Collection<FiscalHistory>();
        }
    }
}
