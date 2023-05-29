using Service_Billing.Data;

namespace Service_Billing.Models
{
    public class ClientTeamRepository : IClientTeamRepository
    {
        private readonly ServiceBillingContext _billingContext;
        public ClientTeamRepository(ServiceBillingContext billingContext)
        {
            _billingContext = billingContext;
        }
        public IEnumerable<ClientTeam> AllTeams => _billingContext.clientTeams.OrderBy(b => b.teamName);

        public ClientTeam? GetTeamById(int id)
        {
            return _billingContext.clientTeams.FirstOrDefault(t => t.teamId == id);
        }

        public IEnumerable<ClientTeam> GetTeamsByFinancialContact(string contact)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ClientTeam> GetTeamsByPrimaryContact(string primaryContact)
        {
            throw new NotImplementedException();
        }
    }
}
