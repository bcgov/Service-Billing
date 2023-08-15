using MediatR;

namespace ServiceBilling.API.Application.Features.ServiceCategories.Queries.GetServiceCategoriesExport
{
    public class GetServiceCategoriesExportQuery : IRequest<ServiceCategoriesExportFileVm>
    {
    }
}
