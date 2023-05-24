using AutoMapper;
using ServiceBilling.API.Application.Features.ClientAccounts.Commands.CreateClientAccount;
using ServiceBilling.API.Application.Features.ClientAccounts.Queries.GetClientAccountDetail;
using ServiceBilling.API.Application.Features.ClientAccounts.Queries.GetClientsAccountList;
using ServiceBilling.API.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBilling.API.Application.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ClientAccount, ClientAccountsListVm>().ReverseMap();
            CreateMap<ClientAccount, ClientAccountDetailVm>().ReverseMap();
            CreateMap<ClientTeam, ClientTeamDto>();

            CreateMap<ClientAccount, CreateClientAccountCommand>();
        }
    }
}
