using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Service_Billing.Models.Repositories;
using Service_Billing.Services.GraphApi;

namespace Service_Billing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeopleController : ControllerBase
    {
        private readonly IPeopleRepository _peopleRepository;
        private readonly IConfiguration _configuration;
        private readonly IGraphApiService _graphApiService;

        public PeopleController(
            IPeopleRepository peopleRepository,
            IConfiguration configuration,
            IGraphApiService graphApiService)
        {
            _peopleRepository = peopleRepository;
            _configuration = configuration;
            _graphApiService = graphApiService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var people = _peopleRepository.AllPeople.ToList();
            return Ok(people);
        }

        [HttpPost]
        public async Task<IActionResult> SyncAsync()
        {
            var people = _peopleRepository.AllPeople.ToList();

            var cca = ConfidentialClientApplicationBuilder
                   .Create(_configuration.GetSection("AzureAd")["ClientId"])
                   .WithClientSecret(_configuration.GetSection("AzureAd")["ClientSecret"])
                   .WithAuthority(new Uri($"https://login.microsoftonline.com/{_configuration.GetSection("AzureAd")["TenantId"]}"))
            .Build();

            foreach (var person in people)
            {
                var graphUser = await _graphApiService.Me(person.Id.ToString(), cca);
                person.DisplayName = graphUser.DisplayName;
                person.Mail = graphUser.Mail;
                await _peopleRepository.Update(person);
            }

            return Ok();
        }

    }
}
