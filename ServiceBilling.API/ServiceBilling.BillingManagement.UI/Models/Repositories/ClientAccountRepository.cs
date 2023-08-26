namespace ServiceBilling.BillingManagement.UI.Models.Repositories
{
    public class ClientAccountRepository : IClientAccountRepository
    {
        public int AddClientAccount(ClientAccount account)
        {
            throw new NotImplementedException();
        }

        public void CreateClientAccount(ClientAccount account)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ClientAccount> GetAll()
        {
            throw new NotImplementedException();
        }

        public ClientAccount? GetClientAccount(int accountId)
        {
            throw new NotImplementedException();
        }

        public int GetClientIdFromClientNumber(int clientNumber)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ClientAccount> SearchClientAccounts(string queryString)
        {
            throw new NotImplementedException();
        }

        public void UpdateClientAccount(ClientAccount account)
        {
            throw new NotImplementedException();
        }
    }
}
