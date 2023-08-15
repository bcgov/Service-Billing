using AutoMapper;
using ServiceBilling.API.Application.Features.Bills.Commands;
using ServiceBilling.API.Application.Features.ClientAccounts.Commands.CreateClientAccount;
using ServiceBilling.API.Application.Features.ClientAccounts.Queries.GetClientAccountDetail;
using ServiceBilling.API.Application.Features.ClientAccounts.Queries.GetClientsAccountList;
using ServiceBilling.API.Application.Features.ClientTeams.Commands.CreateClientTeam;
using ServiceBilling.API.Application.Features.ServiceCategories.Queries.GetServiceCategoriesExport;
using ServiceBilling.API.Application.Features.ServiceCategories.Queries.GetServiceCategoryList;
using ServiceBilling.API.Domain.Entities;

namespace ServiceBilling.API.Application.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ClientAccount, ClientAccountsListVm>().ReverseMap();
            CreateMap<CreateClientAccountCommand, ClientAccount>().ReverseMap();
            CreateMap<ClientAccount, ClientAccountDetailVm>().ReverseMap();

            CreateMap<ServiceCategory, ServiceCategoryListVm>().ReverseMap();
            CreateMap<ServiceCategory, ServiceCategoriesExportDto>().ReverseMap();

            CreateMap<ClientTeam, ClientTeamDto>();

            CreateMap<ClientAccount, CreateClientAccountCommand>();

            CreateMap<CreateClientTeamCommand, ClientTeam>().ReverseMap();

            CreateMap<CreateBillCommand, Bill>().ReverseMap();
        }
    }
}
