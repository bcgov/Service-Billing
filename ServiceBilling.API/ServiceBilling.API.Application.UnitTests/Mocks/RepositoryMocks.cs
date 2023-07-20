using EmptyFiles;
using Moq;
using ServiceBilling.API.Application.Contracts.Persistence;
using ServiceBilling.API.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBilling.API.Application.UnitTests.Mocks
{
    public class RepositoryMocks
    {
        public static Mock<IAsyncRepository<ServiceCategory>> GetServiceCategoryRepository()
        {
            var serviceCategories = new List<ServiceCategory>
            {
                new ServiceCategory
                {
                    Name = "Sample"
                },
            };

            var mockServiceCategoryRepository = new Mock<IAsyncRepository<ServiceCategory>>();
            mockServiceCategoryRepository.Setup(repo => repo.ListAllAsync()).ReturnsAsync(serviceCategories);

            return mockServiceCategoryRepository;
        }
    }
}
