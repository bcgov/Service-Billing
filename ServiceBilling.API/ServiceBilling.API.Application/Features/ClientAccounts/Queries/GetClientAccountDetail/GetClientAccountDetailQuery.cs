using MediatR;

namespace ServiceBilling.API.Application.Features.ClientAccounts.Queries.GetClientAccountDetail
{
    public class GetClientAccountDetailQuery : IRequest<ClientAccountDetailVm>
    {
        public Guid Id { get; set; }
    }
}
