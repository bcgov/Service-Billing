using Microsoft.AspNetCore.Mvc;
using Service_Billing.Models.Repositories;

namespace Service_Billing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeopleController : ControllerBase
    {
        private readonly IBillRepository _billRepository;

        public PeopleController(IBillRepository billRepository)
        {
            _billRepository = billRepository;
        }

        public IActionResult Get()
        {
            var people = _billRepository.AllBills.Select(b => b.ClientAccount.PrimaryContact).ToList();

            return Ok(people); 
        }
    }
}
