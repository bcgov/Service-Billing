using Microsoft.EntityFrameworkCore;
using Service_Billing.Data;
using Service_Billing.Models;

namespace Service_Billing.Models.Repositories
{
    public class ClientAccountRepositry : IClientAccountRepository
    {
        private readonly ServiceBillingContext _context;
        public ClientAccountRepositry(ServiceBillingContext context)
        {
            _context = context;
        }

        public IEnumerable<ClientAccount> GetAll()
        {
            return _context.ClientAccounts.OrderBy(c => c.Name);
        }

        public ClientAccount? GetClientAccount(int accountId)
        {
            return _context.ClientAccounts.FirstOrDefault(c => c.Id == accountId);
        }

        public IEnumerable<ClientAccount> SearchClientAccounts(string queryString)
        {
            return _context.ClientAccounts.Where(c => c.Name.Contains(queryString)).OrderBy(c => c.Name);
        }

        public int AddClientAccount(ClientAccount account)
        {
            _context.AddAsync(account);
            _context.SaveChanges();

            return account.Id;
        }

        public int GetClientIdFromClientNumber(int clientNumber)
        {
            ClientAccount account = _context.ClientAccounts.FirstOrDefault(x => x.ClientNumber == clientNumber);
            if (account != null)
                return account.Id;
            else return 0;
        }

        public IEnumerable<ClientAccount> GetAccountsByContactName(string contactName)
        {
            List<ClientTeam> userTeams = _context.ClientTeams.Where(x => x.PrimaryContact == contactName).ToList();
            userTeams.AddRange(_context.ClientTeams.Where(x => x.FinancialContact == contactName));
            userTeams.AddRange(_context.ClientTeams.Where(x => x.Approver == contactName));
            List<ClientAccount> accounts = new List<ClientAccount>();

            foreach (ClientTeam team in userTeams)
            {
                accounts.AddRange(GetAll().Where(x => x.TeamId == team.Id));
            }

            return accounts.Distinct();
        }

        public void Update(ClientAccount account)
        {
            _context.Update(account);
            _context.SaveChanges(true);
        }

        public void Approve(ClientAccount account)
        {
            account.IsApprovedByEA = true;
            _context.Update(account);
            _context.SaveChanges(true);
        }

        public IEnumerable<ClientAccount> GetInactiveAccounts()
        {
            return _context.ClientAccounts.Where(x => !x.IsActive);
        }
    }
}
