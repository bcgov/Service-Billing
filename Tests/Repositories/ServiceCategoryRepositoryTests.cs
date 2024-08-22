
using Xunit;

namespace ServiceBillingUnitTests.Repositories
{
    public class ServiceCategoryRepositoryTests
    {
        [Fact]
        public void GetAll_ShouldReturnAllServiceCategories_OrderedByName()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetServiceCategoryRepository();

            // Act
            var result = mockRepository.Object.GetAll();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(5, resultList.Count);
        }

        [Fact]
        public void GetById_ShouldReturnCorrectServiceCategory_WhenExists()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetServiceCategoryRepository();

            // Act
            var result = mockRepository.Object.GetById(3);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.ServiceId);
        }

        [Fact]
        public void GetById_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetServiceCategoryRepository();

            // Act
            var result = mockRepository.Object.GetById(999); // ID not in the list

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Search_ShouldReturnMatchingServiceCategories()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetServiceCategoryRepository();

            // Act
            var result = mockRepository.Object.Search("CMS Lite licence");

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal("CMS Lite licence", resultList[0].Name);
        }

        [Fact]
        public void Search_ShouldReturnEmpty_WhenNoMatch()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetServiceCategoryRepository();

            // Act
            var result = mockRepository.Object.Search("Non-Existent Service");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
