using Microsoft.EntityFrameworkCore;
using Service_Billing.Data;
using Service_Billing.Models;

namespace Service_Billing.Models.Repositories
{
    public class ClientAccountRepository : IClientAccountRepository
    {
        private readonly ServiceBillingContext _context;
        private readonly IChangeLogRepository _changeLogRepository;
        public ClientAccountRepository(ServiceBillingContext context, IChangeLogRepository changeLogRepository)
        {
            _context = context;
            _changeLogRepository = changeLogRepository;
        }

        public IEnumerable<ClientAccount> GetAll()
        {
            return _context.ClientAccounts.AsNoTracking().OrderBy(c => c.Name);
        }

        public ClientAccount? GetClientAccount(int accountId)
        {
            return _context.ClientAccounts.Include(c => c.Bills).ThenInclude(b => b.ServiceCategory).FirstOrDefault(c => c.Id == accountId);
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
            List<ClientAccount> userAccounts = _context.ClientAccounts.Where(x => x.PrimaryContact == contactName).ToList();
            userAccounts.AddRange(_context.ClientAccounts.Where(x => x.FinancialContact == contactName));
            userAccounts.AddRange(_context.ClientAccounts.Where(x => x.Approver == contactName));
          
            return userAccounts.Distinct();
        }

        public async Task Update(ClientAccount editedAccount, string userName = "system")
        {
            ClientAccount? account = GetClientAccount(editedAccount.Id);
            if (account == null)
            {
                throw new Exception("Could not retrieve account from database");
            }
            await _changeLogRepository.MakeChangeLogEntry(editedAccount, userName);
            _context.Update(account);
            await _context.SaveChangesAsync(true);
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

        public IEnumerable<ClientAccount> GetAccountsByOrgId(int orgId)
        {
            return _context.ClientAccounts.Where(x => x.OrganizationId == orgId);
        }
    }
}
