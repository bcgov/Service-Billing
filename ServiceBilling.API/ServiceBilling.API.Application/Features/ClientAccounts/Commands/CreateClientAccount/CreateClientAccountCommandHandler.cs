using AutoMapper;
using MediatR;
using ServiceBilling.API.Application.Contracts.Persistence;
using ServiceBilling.API.Domain.Entities;

namespace ServiceBilling.API.Application.Features.ClientAccounts.Commands.CreateClientAccount
{
    public class CreateClientAccountCommandHandler : IRequestHandler<CreateClientAccountCommand, Guid>
    {
        private readonly IAsyncRepository<ClientAccount> _clientAccountRepository;
        private readonly IAsyncRepository<ClientTeam> _clientTeamRepository;
        private readonly IMapper _mapper;

        public CreateClientAccountCommandHandler(
            IAsyncRepository<ClientAccount> clientAccountRepository,
            IAsyncRepository<ClientTeam> clientTeamRepository,
            IMapper mapper)
        {
            _clientAccountRepository = clientAccountRepository;
            _clientTeamRepository = clientTeamRepository;
            _mapper = mapper;
        }

        public async Task<Guid> Handle(CreateClientAccountCommand request, CancellationToken cancellationToken)
        {

            // var clientTeam = await _clientTeamRepository.GetByIdAsync(request.ClientTeamId);

            // var ct = await _clientAccountRepository.ListAllAsync();

            var clientAccount = _mapper.Map<ClientAccount>(request);

            // create a validator object
            // await the result of a call to validateasync

            // clientAccount.ClientTeam = clientTeam;

            clientAccount = await _clientAccountRepository.AddAsync(clientAccount);

            return clientAccount.ClientAccountId;
        }
    }
}
