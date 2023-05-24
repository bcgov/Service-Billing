using AutoMapper;
using MediatR;
using ServiceBilling.API.Application.Contracts.Persistence;
using ServiceBilling.API.Domain.Entities;

namespace ServiceBilling.API.Application.Features.ClientAccounts.Queries.GetClientsAccountList
{
    public class GetClientAccountsListQueryHandler : IRequestHandler<GetClientAccountsListQuery, IList<ClientAccountsListVm>>
    {
        private readonly IAsyncRepository<ClientAccount> _clientAccountRepository;
        private readonly IMapper _mapper;

        public GetClientAccountsListQueryHandler(IMapper mapper, IAsyncRepository<ClientAccount> clientAccountRepository)
        {
            _mapper = mapper;
            _clientAccountRepository = clientAccountRepository;
        }

        public async Task<IList<ClientAccountsListVm>> Handle(GetClientAccountsListQuery request, CancellationToken cancellationToken)
        {
            var allClientAccounts = await _clientAccountRepository.ListAllAsync();
            return _mapper.Map<List<ClientAccountsListVm>>(allClientAccounts);
        }
    }
}
