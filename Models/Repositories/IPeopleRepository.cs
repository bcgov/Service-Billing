namespace Service_Billing.Models.Repositories
{
    public interface IPeopleRepository
    {
        IEnumerable<Person> AllPeople { get; }
        Task Update(Person person);

        Task<int> AddPerson(Person person);
        Task<int> AddPersonByDisplayName(string displayName);
        Person? GetPersonById(int id);

        Person? GetPersonByDisplayName(string displayName);
    }
}
