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

        public ClientTeam? GetTeamByName(string name)
        {
            return _billingContext.clientTeams.FirstOrDefault(t => t.teamName == name);
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
