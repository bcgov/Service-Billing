using MediatR;
using Microsoft.AspNetCore.Mvc;
using ServiceBilling.API.Application.Features.ClientAccounts.Commands.CreateClientAccount;
using ServiceBilling.API.Application.Features.ClientAccounts.Queries.GetClientsAccountList;

namespace ServiceBilling.BillingManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientAccountController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClientAccountController(IMediator mediator)
        {
            _mediator = mediator;
        }


        // [Authorize]
        [HttpGet("all", Name = "GetAllClientAccounts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ClientAccountsListVm>>> GetAllClientAccounts()
        {
            var dtos = await _mediator.Send(new GetClientAccountsListQuery());
            return Ok(dtos);
        }

        [HttpPost(Name = "AddClientAccount")]
        public async Task<ActionResult<CreateClientAccountCommandResponse>> Create([FromBody] CreateClientAccountCommand createClientAccountCommand)
        {
            var response = await _mediator.Send(createClientAccountCommand);
            return Ok(response);
        }
    }
}
