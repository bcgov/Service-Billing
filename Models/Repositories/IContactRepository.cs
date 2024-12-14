namespace Service_Billing.Models.Repositories
{
    public interface IContactRepository
    {
        Task AddContact(Contact contact);
        Contact Get(int id);
        Contact GetByEmail(string email);

        IEnumerable<Contact> GetContactsByAccountId(int accountId);
        void UpdateContact(Contact contact);

        void DeleteContact(Contact contact);

    }
}
