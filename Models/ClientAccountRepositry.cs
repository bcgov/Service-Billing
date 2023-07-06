using Microsoft.EntityFrameworkCore;
using Service_Billing.Data;

namespace Service_Billing.Models
{
    public class ClientAccountRepositry : IClientAccountRepository
    {
        private readonly ServiceBillingContext _context;
        public ClientAccountRepositry(ServiceBillingContext context)
        {
            _context = context;
        }

        public void CreateClientAccount(ClientAccount account)
        {
            throw new NotImplementedException();
        }

        public void UpdateClientAccount(ClientAccount account) 
        {
            throw new NotImplementedException();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
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
            var x =  _context.ClientAccounts.Where(c => c.Name.Contains(queryString)).OrderBy(c => c.Name);
            var y = x.ToList();
            return x;
        }
    }
}
