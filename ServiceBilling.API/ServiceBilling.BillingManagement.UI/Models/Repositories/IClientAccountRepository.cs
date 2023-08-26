using ServiceBilling.BillingManagement.UI.Models;

namespace ServiceBilling.BillingManagement.UI.Models.Repositories
{
    public interface IClientAccountRepository
    {
        IEnumerable<ClientAccount> GetAll();
        void CreateClientAccount(ClientAccount account);
        void UpdateClientAccount(ClientAccount account);
        ClientAccount? GetClientAccount(int accountId);
        IEnumerable<ClientAccount> SearchClientAccounts(string queryString);
        int AddClientAccount(ClientAccount account);
        int GetClientIdFromClientNumber(int clientNumber);
    }
}
