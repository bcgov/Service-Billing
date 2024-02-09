using Microsoft.AspNetCore.Mvc;
using Service_Billing.Models.Repositories;

namespace Service_Billing.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeopleController : ControllerBase
    {
        private readonly IBillRepository _billRepository;

        public PeopleController(ILogger<PeopleController> logger,
            IBillRepository billRepository)
        {
            _billRepository = billRepository;
        }

        public IActionResult Get()
        {
            //var people = _billRepository
            //    .AllBills
            //        .Where(b => b.ClientAccount is not null 
            //                    && b.ClientAccount.Team is not null 
            //                    && b.ClientAccount.Team.PrimaryContact is not null)
            //            .Select(b => b.ClientAccount.Team.PrimaryContact);


            //return Ok(people);
            return Ok();
        }
    }
}
