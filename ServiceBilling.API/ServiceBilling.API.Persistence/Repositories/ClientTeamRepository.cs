using ServiceBilling.API.Application.Contracts;
using ServiceBilling.API.Application.Contracts.Persistence;
using ServiceBilling.API.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBilling.API.Persistence.Repositories
{
    public class ClientTeamRepository : BaseRepository<ClientTeam>, IClientTeamRepository
    {
        public ClientTeamRepository(DataContext dataContext) : base(dataContext)
        {         
        }
    }
}
