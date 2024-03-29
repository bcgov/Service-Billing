﻿using ServiceBilling.API.Application.Contracts.Persistence;
using ServiceBilling.API.Domain.Entities;

namespace ServiceBilling.API.Persistence.Repositories
{
    public class ClientTeamRepository : BaseRepository<ClientTeam>, IClientTeamRepository
    {
        public ClientTeamRepository(DataContext dbContext) : base(dbContext)
        {         
        }
    }
}
