namespace Service_Billing.Models
{
    public interface IClientTeamRepository
    {
        public IEnumerable<ClientTeam> AllTeams { get; }
        public ClientTeam? GetTeamById(int id);
        public IEnumerable<ClientTeam> GetTeamsByFinancialContact(string contact);
        public IEnumerable<ClientTeam> GetTeamsByPrimaryContact(string primaryContact);
    }
}
