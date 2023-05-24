using MediatR;

namespace ServiceBilling.API.Application.Features.ClientAccounts.Commands.CreateClientAccount
{
    public class CreateClientAccountCommand : IRequest<Guid>
    {
        public int Number { get; set; }
        public string Name { get; set; }
    }
}
