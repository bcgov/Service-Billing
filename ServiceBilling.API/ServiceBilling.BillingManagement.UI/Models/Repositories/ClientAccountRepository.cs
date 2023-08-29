using Microsoft.EntityFrameworkCore;

namespace ServiceBilling.BillingManagement.UI.Models.Repositories
{
    public class ClientAccountRepository : IClientAccountRepository
    {
        private readonly DataContext _dataContext;

        public ClientAccountRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public IEnumerable<ClientAccount> GetAll()
        {
            return _dataContext.ClientAccounts.OrderBy(c => c.Name);
        }

        public ClientAccount? GetClientAccount(int accountId)
        {
            return _dataContext.ClientAccounts.FirstOrDefault(c => c.Id == accountId);
        }

        public IEnumerable<ClientAccount> SearchClientAccounts(string queryString)
        {
            var x = _dataContext.ClientAccounts.Where(c => c.Name.Contains(queryString)).OrderBy(c => c.Name);
            var y = x.ToList();
            return x;
        }

        public int AddClientAccount(ClientAccount account)
        {
           _dataContext.AddAsync(account);
           _dataContext.SaveChanges();

            return account.Id;
        }

        public int GetClientIdFromClientNumber(int clientNumber)
        {
            ClientAccount account = _dataContext.ClientAccounts.FirstOrDefault(x => x.ClientNumber == clientNumber);
            if (account != null)
                return account.Id;
            else return 0;
        }

    }
}

