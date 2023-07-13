using ServiceBilling.API.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBilling.API.Domain.Entities
{
    public class Bill : AuditableEntity
    {
        public Guid Id { get; set; }
        // public Guid ClientAccountId { get; set; }
        public string Title { get; set; }
        public string IdirOrUrl { get; set; }

        // public Int16? serviceCategoryId { get; set; }

        public decimal Amount { get; set; }

        public string FiscalPeriod { get; set; }
        //  public decimal Unit_Price; //comes from service category lookup
        public decimal Quantity { get; set; }

        public string TicketNumber { get; set; }
        // public string Requester { get; set; }
        // public DateTime? billingCycle { get; set; }
    }
}
