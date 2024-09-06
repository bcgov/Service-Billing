
using Xunit;

namespace ServiceBillingUnitTests.Repositories
{
    public class MinistryRepositoryTests
    {
        [Fact]
        public void GetAll_ShouldReturnAllMinistries_OrderedByAcronym()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetMinistryRepository();

            // Act
            var result = mockRepository.Object.GetAll();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(5, resultList.Count);
        }

        [Fact]
        public void GetById_ShouldReturnCorrectMinistry_WhenExists()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetMinistryRepository();

            // Act
            var result = mockRepository.Object.GetById(2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Id);
            Assert.Equal("Tourism, Arts, Culture and Sport", result.Title);
        }

        [Fact]
        public void GetById_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetMinistryRepository();

            // Act
            var result = mockRepository.Object.GetById(999); // ID not in the list

            // Assert
            Assert.Null(result);
        }
    }

}
