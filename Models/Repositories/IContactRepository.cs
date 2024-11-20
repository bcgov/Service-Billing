namespace Service_Billing.Models.Repositories
{
    public interface IContactRepository
    {
        void Add(Contact contact);
        Contact Get(int id);
        Contact GetByEmail(string email);

    }
}
