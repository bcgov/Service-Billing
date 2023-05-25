using ServiceBilling.API.Domain.Common;

namespace ServiceBilling.API.Domain.Entities
{
    public class ServiceCategory : AuditableEntity
    {
        public Guid ServiceCategoryId { get; set; }
        public string Name { get; set; }
    }
}
