using AutoMapper;
using MediatR;
using ServiceBilling.API.Application.Contracts.Persistence;
using ServiceBilling.API.Domain.Entities;

namespace ServiceBilling.API.Application.Features.ClientAccounts.Queries.GetClientAccountDetail
{
    public class GetClientAccountDetailQueryHandler : IRequestHandler<GetClientAccountDetailQuery, ClientAccountDetailVm>
    {
        private readonly IAsyncRepository<ClientAccount> _clientAccountRepository;
        private readonly IAsyncRepository<ClientTeam> _clientTeamRepository;
        private readonly IMapper _mapper;

        public GetClientAccountDetailQueryHandler(
            IAsyncRepository<ClientAccount> clientAccountRepository,
            IAsyncRepository<ClientTeam> clientTeamRepository,
            IMapper mapper
            )
        {
            _clientAccountRepository = clientAccountRepository;
            _clientTeamRepository = clientTeamRepository;
            _mapper = mapper;
        }

        public async Task<ClientAccountDetailVm> Handle(GetClientAccountDetailQuery request, CancellationToken cancellationToken)
        {
            var @clientAccount = await _clientAccountRepository.GetByIdAsync(request.Id);
            var clientAccountDto = _mapper.Map<ClientAccountDetailVm>(@clientAccount);

            var clientTeam = await _clientTeamRepository.GetByIdAsync(@clientAccount.ClientTeamId);

            if (clientTeam == null)
            {
                // throw new NotFoundException(nameof(ClientAccount), request.Id);
            }

            clientAccountDto.ClientTeam = _mapper.Map<ClientTeamDto>(clientTeam);

            return clientAccountDto;
        }
    }
}
