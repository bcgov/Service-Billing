
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Service_Billing.Data;
using Service_Billing.Services.GraphApi;

namespace Service_Billing.Models.Repositories
{
    public class PeopleRepository : IPeopleRepository
    {
        private readonly ServiceBillingContext _billingContext;
        private readonly IGraphApiService _graphApiService;
        private readonly IConfiguration _configuration;


        public PeopleRepository(ServiceBillingContext billingContext, 
        IGraphApiService graphApiService,
        IConfiguration configuration)
        {
            _billingContext = billingContext;
            _graphApiService = graphApiService;
            _configuration = configuration; 
        }

        public IEnumerable<Person> AllPeople => _billingContext.People.AsNoTracking();
        public Person? GetPersonById(int id)
        {
            return _billingContext.People.SingleOrDefault(p => p.Id == id);
        }

        public async Task<int> AddPerson(Person person)
        {
            await _billingContext.People.AddAsync(person);
            await _billingContext.SaveChangesAsync();
            return person.Id;
        }

        public async Task<int> AddPersonByDisplayName(string displayName)
        {
            var cca = ConfidentialClientApplicationBuilder
                       .Create(_configuration.GetSection("AzureAd")["ClientId"])
                       .WithClientSecret(_configuration.GetSection("AzureAd")["ClientSecret"])
                       .WithAuthority(new Uri($"https://login.microsoftonline.com/{_configuration.GetSection("AzureAd")["TenantId"]}"))
                        .Build();

            GraphApiListResponse<GraphUser> users = await _graphApiService.GetUsersByDisplayName(displayName, cca);
            if (users == null || users.Value.Count == 0)
                throw new Exception($"A graph user could not be discerned for {displayName}.");
            GraphUser user = users.Value.First();
            Person person = new Person();
            person.Name = $"{user.GivenName} {user.Surname}";
            person.DisplayName = user.DisplayName;
            person.Mail = user.Mail;

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
