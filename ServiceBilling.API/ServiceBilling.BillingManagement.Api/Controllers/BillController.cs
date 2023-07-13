using MediatR;
using Microsoft.AspNetCore.Mvc;
using ServiceBilling.API.Application.Features.Bills.Commands;

namespace ServiceBilling.BillingManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BillController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost(Name = "AddBill")]
        public async Task<ActionResult<CreateBillCommand>> Create([FromBody] CreateBillCommand createBillCommand)
        {
            var response = await _mediator.Send(createBillCommand);
            return Ok(response);
        }
    }
}
