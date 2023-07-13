using AutoMapper;
using MediatR;
using ServiceBilling.API.Application.Contracts.Persistence;
using ServiceBilling.API.Domain.Entities;

namespace ServiceBilling.API.Application.Features.ClientTeams.Commands.CreateClientTeam
{
    public class CreateClientTeamCommandHandler : IRequestHandler<CreateClientTeamCommand, Guid>
    {
        private readonly IAsyncRepository<ClientTeam> _clientTeamRepository;
        private readonly IMapper _mapper;

        public CreateClientTeamCommandHandler(
            IAsyncRepository<ClientTeam> clientTeamRepository,
            IMapper mapper)
        {
            _clientTeamRepository = clientTeamRepository;
            _mapper = mapper;
        }

        public async Task<Guid> Handle(CreateClientTeamCommand request, CancellationToken cancellationToken)
        {
            var clientTeam = _mapper.Map<ClientTeam>(request);

            await _clientTeamRepository.AddAsync(clientTeam);

            return clientTeam.ClientTeamId;
        }
    }
}
