namespace Service_Billing.Models.Repositories
{
    public interface IClientAccountRepository
    {
        IEnumerable<ClientAccount> GetAll();
        ClientAccount? GetClientAccount(int accountId);
        IEnumerable<ClientAccount> SearchClientAccounts(string queryString);
        int AddClientAccount(ClientAccount account);
        int GetClientIdFromClientNumber(int clientNumber);
        IEnumerable<ClientAccount> GetAccountsByContactName(string contactName);
    }
}
