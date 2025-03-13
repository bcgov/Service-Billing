using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Graph;
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
            return _context.ClientAccounts.Include(c => c.Contacts).ThenInclude(p => p.Person).AsNoTracking().OrderBy(c => c.Name);
        }

        public ClientAccount? GetClientAccount(int accountId)
        {
            return _context.ClientAccounts
            .Include(c => c.Bills).ThenInclude(b => b.ServiceCategory)
            .Include(c => c.Contacts).ThenInclude(p => p.Person)
            .FirstOrDefault(c => c.Id == accountId);
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
            EntityEntry entry = await _changeLogRepository.MakeChangeLogAndReturnEntry(editedAccount, userName);
            ClientAccount? account = entry.Entity as ClientAccount;
            if (account != null)
            {
                _context.Update(account);
                await _context.SaveChangesAsync(true);
            }
            else
                throw new Exception($"Something went wrong while trying to update ClientAccount with Id {editedAccount.Id}");        
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
