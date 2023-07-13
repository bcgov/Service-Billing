
using AutoMapper;
using MediatR;
using ServiceBilling.API.Application.Contracts.Persistence;
using ServiceBilling.API.Domain.Entities;

namespace ServiceBilling.API.Application.Features.ServiceCategories.Queries.GetServiceCategoryList
{
    public class GetServiceCategoryListQueryHandler : IRequestHandler<GetServiceCategoryListQuery, List<ServiceCategoryListVm>>
    {
        private readonly IMapper _mapper;
        private readonly IAsyncRepository<ServiceCategory> _serviceCategoryRepository;

        public GetServiceCategoryListQueryHandler(IMapper mapper, IAsyncRepository<ServiceCategory> serviceCategoryRepository)
        {
            _mapper = mapper;
            _serviceCategoryRepository = serviceCategoryRepository;
        }

        public async Task<List<ServiceCategoryListVm>> Handle(GetServiceCategoryListQuery request, CancellationToken cancellationToken)
        {
            var allServiceCategories = await _serviceCategoryRepository.ListAllAsync();
            return _mapper.Map<List<ServiceCategoryListVm>>(allServiceCategories);
        }
    }
}
