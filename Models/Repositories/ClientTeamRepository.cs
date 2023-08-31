using Microsoft.CodeAnalysis.Elfie.Model.Strings;
using Service_Billing.Data;
using Service_Billing.Models;

namespace Service_Billing.Models.Repositories
{
    public class ClientTeamRepository : IClientTeamRepository
    {
        private readonly ServiceBillingContext _billingContext;
        public ClientTeamRepository(ServiceBillingContext billingContext)
        {
            _billingContext = billingContext;
        }
        public IEnumerable<ClientTeam> AllTeams => _billingContext.ClientTeams.OrderBy(b => b.Name);

        public ClientTeam? GetTeamById(int? id)
        {
            if (id == null) return null;
            return _billingContext.ClientTeams.FirstOrDefault(t => t.Id == id);
        }

        public ClientTeam? GetTeamByName(string name)
        {
            return _billingContext.ClientTeams.FirstOrDefault(t => t.Name == name);
        }

        public IEnumerable<ClientTeam> GetTeamsByFinancialContact(string contact)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ClientTeam> GetTeamsByPrimaryContact(string primaryContact)
        {
            throw new NotImplementedException();
        }

        public int Add(ClientTeam team)
        {
            _billingContext.AddAsync(team);
            _billingContext.SaveChanges();
            
            return team.Id;
        }
    }
}
