using MediatR;

namespace ServiceBilling.API.Application.Features.ClientAccounts
{
    public class GetClientAccountDetailQuery : IRequest<ClientAccountDetailVm>
    {
        public Guid Id { get; set; }
    }
}
