using ServiceBilling.API.Domain.Common;

namespace ServiceBilling.API.Domain.Entities
{
    public class ClientTeam : AuditableEntity
    {
        public ClientTeam()
        {
            ClientAccounts = new List<ClientAccount>();
        }

        public Guid ClientTeamId { get; set; }
        public string ClientTeamName { get; set; }
        public List<ClientAccount> ClientAccounts { get; set; }
    }
}