namespace ServiceBilling.API.Application.Features.ClientAccounts.Queries.GetClientsAccountList
{
    public class ClientAccountsListVm
    {
        public Guid ClientAccountId { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
    }
}
