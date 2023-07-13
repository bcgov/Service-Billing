using MediatR;
using Microsoft.AspNetCore.Mvc;
using ServiceBilling.API.Application.Features.ClientTeams.Commands.CreateClientTeam;

namespace ServiceBilling.BillingManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientTeamController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClientTeamController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost(Name = "AddClientTeam")]
        public async Task<ActionResult<CreateClientTeamCommand>> Create([FromBody] CreateClientTeamCommand createClientTeamCommand)
        {
            var response = await _mediator.Send(createClientTeamCommand);
            return Ok(response);
        }
    }
}
