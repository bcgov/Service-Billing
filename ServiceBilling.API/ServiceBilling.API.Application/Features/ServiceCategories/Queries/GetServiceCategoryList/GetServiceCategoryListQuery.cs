using MediatR;


namespace ServiceBilling.API.Application.Features.ServiceCategories.Queries.GetServiceCategoryList
{
    public class GetServiceCategoryListQuery : IRequest<List<ServiceCategoryListVm>>
    {
    }
}
