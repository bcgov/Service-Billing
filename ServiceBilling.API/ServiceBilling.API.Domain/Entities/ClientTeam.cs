using ServiceBilling.API.Domain.Common;

namespace ServiceBilling.API.Domain.Entities
{
    public class ClientTeam : AuditableEntity
    {
        public Guid ClientTeamId { get; set; }
        public ClientAccount ClientAccount { get; set; }
    }
}