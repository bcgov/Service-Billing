using MediatR;

namespace ServiceBilling.API.Application.Features.ClientTeams.Commands.CreateClientTeam
{
    public class CreateClientTeamCommand : IRequest<Guid>
    {
        public string ClientTeamName { get; set; }
    }
}
