using AutoMapper;
using Moq;
using ServiceBilling.API.Application.Contracts.Persistence;
using ServiceBilling.API.Application.Features.ServiceCategories.Queries.GetServiceCategoryList;
using ServiceBilling.API.Application.Profiles;
using ServiceBilling.API.Application.UnitTests.Mocks;
using ServiceBilling.API.Domain.Entities;
using Shouldly;

namespace ServiceBilling.API.Application.UnitTests.ServiceCategories.Queries
{
    public class GetServiceCategoryListQueryHandlerTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IAsyncRepository<ServiceCategory>> _mockServiceCategoriesRepository;

        public GetServiceCategoryListQueryHandlerTests()
        {
            _mockServiceCategoriesRepository = RepositoryMocks.GetServiceCategoryRepository();
            var configurationProvider = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            _mapper = configurationProvider.CreateMapper();
        }

        [Fact]
        public async Task GetServiceCategoriesListTest()
        {
            var handler = new GetServiceCategoryListQueryHandler(_mapper, _mockServiceCategoriesRepository.Object);

            var result = await handler.Handle(new GetServiceCategoryListQuery(), CancellationToken.None);

            result.ShouldBeOfType<List<ServiceCategoryListVm>>(); 
            
            result.Count.ShouldBe(1);
        }
    }
}
