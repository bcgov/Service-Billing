using AutoMapper;
using MediatR;
using ServiceBilling.API.Application.Contracts.Infrastructure;
using ServiceBilling.API.Application.Contracts.Persistence;
using ServiceBilling.API.Domain.Entities;

namespace ServiceBilling.API.Application.Features.ServiceCategories.Queries.GetServiceCategoriesExport
{
    public class GetServiceCategoriesExportQueryHandler : IRequestHandler<GetServiceCategoriesExportQuery, ServiceCategoriesExportFileVm>
    {
        private readonly IMapper _mapper;
        private readonly IAsyncRepository<ServiceCategory> _serviceCategoryRepository;
        private readonly ICsvExporter _csvExporter;

        public GetServiceCategoriesExportQueryHandler(IMapper mapper, IAsyncRepository<ServiceCategory> serviceCategoryRepository, ICsvExporter csvExporter)
        {
            _mapper = mapper;
            _serviceCategoryRepository = serviceCategoryRepository;
            _csvExporter = csvExporter;
        }

        public async Task<ServiceCategoriesExportFileVm> Handle(GetServiceCategoriesExportQuery request, CancellationToken cancellationToken)
        {
            var allServiceCategories = _mapper.Map<List<ServiceCategoriesExportDto>>((await _serviceCategoryRepository.ListAllAsync()).OrderBy(x => x.Name));

            var fileData = _csvExporter.ExportServiceCategoriesToCsv(allServiceCategories);

            var serviceCategoriesFileDto = new ServiceCategoriesExportFileVm() { ContentType = "text/csv", Data = fileData, ServiceCategoriesExportFileName = $"{Guid.NewGuid()}.csv" };

            return serviceCategoriesFileDto;
        }
    }
}
