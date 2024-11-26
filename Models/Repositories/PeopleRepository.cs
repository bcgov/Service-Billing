
using Microsoft.EntityFrameworkCore;
using Service_Billing.Data;

namespace Service_Billing.Models.Repositories
{
    public class PeopleRepository : IPeopleRepository
    {
        private readonly ServiceBillingContext _billingContext;
        public PeopleRepository(ServiceBillingContext billingContext)
        {
            _billingContext = billingContext;
        }

        public IEnumerable<Person> AllPeople => _billingContext.People.AsNoTracking();

        public async Task<int> AddPerson(Person person)
        {
            await _billingContext.People.AddAsync(person);
            await _billingContext.SaveChangesAsync();
            return person.Id;
        }

        public Person? GetPersonByDisplayName(string displayName)
        {
            return _billingContext.People.FirstOrDefault(p => p.DisplayName == displayName);
        }

        public async Task Update(Person editedPerson)
        {
            var person = await _billingContext.People.FindAsync(editedPerson.Id);

            if (person == null) throw new Exception("could not retrieve person from database");

            person.DisplayName = editedPerson.DisplayName;
            person.Mail = editedPerson.Mail;

            _billingContext.Update(person);
            await _billingContext.SaveChangesAsync();
        }
    }
}
