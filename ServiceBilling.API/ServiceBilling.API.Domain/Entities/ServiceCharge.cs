using ServiceBilling.API.Domain.Common;

namespace ServiceBilling.API.Domain.Entities
{
    public class ServiceCharge : AuditableEntity
    {
        public Guid ServiceChargeId { get; set; }
        public int Amount { get; set; }
        public int Quantity { get; set; }
        public string ExpenseAuthorityName { get; set; }
        public string TicketNumber { get; set; }
        public string RequesterName { get; set; }
    }
}
