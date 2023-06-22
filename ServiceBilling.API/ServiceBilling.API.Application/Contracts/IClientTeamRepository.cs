using ServiceBilling.API.Application.Contracts.Persistence;
using ServiceBilling.API.Domain.Entities;

namespace ServiceBilling.API.Application.Contracts
{
    public interface IClientTeamRepository : IAsyncRepository<ClientTeam>
    {
    }
}
