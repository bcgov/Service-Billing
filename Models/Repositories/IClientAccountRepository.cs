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
        Task Update(ClientAccount account, string userName, bool saveChanges);
        void Approve(ClientAccount account);
        IEnumerable<ClientAccount> GetInactiveAccounts();
        IEnumerable<ClientAccount> GetAccountsByOrgId(int orgId);
    }
}
