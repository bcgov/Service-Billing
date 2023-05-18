using AutoMapper;
using ServiceBilling.API.Application.Features.ClientAccounts;
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
        }
    }
}
