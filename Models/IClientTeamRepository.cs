namespace Service_Billing.Models
{
    public interface IClientTeamRepository
    {
        public IEnumerable<ClientTeam> AllTeams { get; }
        public ClientTeam? GetTeamByName(string teamName);
        public IEnumerable<ClientTeam> GetTeamsByFinancialContact(string contact);
        public IEnumerable<ClientTeam> GetTeamsByPrimaryContact(string primaryContact);
    }
}
