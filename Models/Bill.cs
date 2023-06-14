﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Billing.Models
{
    public class Bill
    {
        [Key]
        public Int16 id { get; set; }
        public Int16 clientAccountId { get; set; }

        [Display(Name = "Client Name")]
        public string? clientName { get; set; }

        [Display(Name = "Title")]
        public string? title { get; set; }

        [Display(Name = "URL or IDIR")]
        public string? idirOrUrl { get; set; }

        public Int16? serviceCategoryId { get; set; }

        [Display(Name = "Amount")]
        public decimal? amount { get; set; }

        [Display(Name = "Fiscal Period")]
        public string? fiscalPeriod { get; set; }
      //  public decimal Unit_Price; //comes from service category lookup
        public decimal? quantity { get; set; }

        [Column("ticketNumberAndRequesterName")]
        [Display(Name = "Ticket Number")]
        public string? ticketNumberAndRequester { get; set; }

        public DateTime? dateModified { get; set; }
        public DateTime? dateCreated { get; set; }
        public DateTime? billingCycle { get; set; }
        public string? createdBy { get; set; }
    }
}
