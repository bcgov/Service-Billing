namespace ServiceBilling.BillingManagement.UI.Models.Repositories
{
    public class ClientTeamRepository : IClientTeamRepository
    {
        private readonly DataContext _dataContext;
        public ClientTeamRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        public IEnumerable<ClientTeam> AllTeams => _dataContext.ClientTeams.OrderBy(b => b.Name);

        public ClientTeam? GetTeamById(int? id)
        {
            if (id == null) return null;
            return _dataContext.ClientTeams.FirstOrDefault(t => t.Id == id);
        }

        public ClientTeam? GetTeamByName(string name)
        {
            return _dataContext.ClientTeams.FirstOrDefault(t => t.Name == name);
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
            _dataContext.AddAsync(team);
            _dataContext.SaveChanges();

            return team.Id;
        }
    }
}
