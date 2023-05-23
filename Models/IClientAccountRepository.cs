namespace Service_Billing.Models
{
    public interface IClientAccountRepository
    {
        IEnumerable<ClientAccount> GetAll();
        void CreateClientAccount(ClientAccount account);
        ClientAccount? GetClientAccount(int accountId);
        IEnumerable<ClientAccount> SearchClientAccounts(string queryString);

    }
}
