using Service_Billing.Models;

namespace Service_Billing.ViewModels
{
    public class ClientAccountViewModel
    {
        public IEnumerable<ClientAccount> ClientAccounts { get; }
        public IEnumerable<ClientTeam> ClientTeams { get; }
        public Dictionary<int, string> PrimaryContacts { get; }

        public ClientAccountViewModel(IEnumerable<ClientAccount> clients, IEnumerable<ClientTeam> teams)
        {
            ClientAccounts = clients;
            ClientTeams = teams;
            PrimaryContacts = new Dictionary<int, string>();
            BuildPrimaryContactDict();
        }

        private void BuildPrimaryContactDict()
        {
            if (ClientTeams != null && ClientTeams.Any())
            {
                foreach (var client in ClientAccounts)
                {
                    if(client.TeamId == null)
                    {
                        PrimaryContacts.Add(client.Id, "No Account Team");
                        continue;
                    }
                    ClientTeam team = ClientTeams.FirstOrDefault(x => x.Id == client.TeamId);
                    if (team != null)
                    {
                        PrimaryContacts.Add(client.Id, team.PrimaryContact);
                    }
                }
            }
        }
    }
}
