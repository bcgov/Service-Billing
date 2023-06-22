using ServiceBilling.API.Application.Contracts.Persistence;
using ServiceBilling.API.Domain.Entities;

namespace ServiceBilling.API.Persistence.Repositories
{
    public class ClientAccountRepository : BaseRepository<ClientAccount>, IClientAccountRepository
    {
        public ClientAccountRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }
}
