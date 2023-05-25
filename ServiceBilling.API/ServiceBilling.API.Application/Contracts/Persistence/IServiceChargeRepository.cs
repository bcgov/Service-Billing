using ServiceBilling.API.Domain.Entities;

namespace ServiceBilling.API.Application.Contracts.Persistence
{
    public interface IServiceChargeRepository : IAsyncRepository<ServiceCharge>
    {
    }
}
