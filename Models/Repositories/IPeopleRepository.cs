namespace Service_Billing.Models.Repositories
{
    public interface IPeopleRepository
    {
        IEnumerable<Person> AllPeople { get; }
        Task Update(Person person);
    }
}
