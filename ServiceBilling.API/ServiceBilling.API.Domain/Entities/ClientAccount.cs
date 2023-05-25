using ServiceBilling.API.Domain.Common;

namespace ServiceBilling.API.Domain.Entities
{
    public class ClientAccount : AuditableEntity
    {
        public Guid ClientAccountId { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public ClientTeam ClientTeam { get; set; }
        public Guid ClientTeamId { get; set; }
    }
}
