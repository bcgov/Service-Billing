using ServiceBilling.API.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBilling.API.Application.Features.ClientAccounts
{
    public class ClientAccountDetailVm
    {
        public Guid ClientAccountId { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }

        public ClientTeamDto ClientTeam { get; set; }
        public string ClientTeamId { get; set; }
    }
}
