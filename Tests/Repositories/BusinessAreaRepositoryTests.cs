using Xunit;

namespace ServiceBillingUnitTests.Repositories
{
    public class BusinessAreaRepositoryTests
    {
        [Fact]
        public void GetAll_ShouldReturnAllBusinessAreas_OrderedByAcronym()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetBusinessAreaRepository();

            // Act
            var result = mockRepository.Object.GetAll();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(7, resultList.Count);
            Assert.Equal("ANA", resultList[0].Acronym);
            Assert.Equal("Any", resultList[1].Acronym);
            Assert.Equal("DES", resultList[2].Acronym);
            Assert.Equal("DMS", resultList[3].Acronym);
            Assert.Equal("Multi", resultList[4].Acronym);
            Assert.Equal("OSS", resultList[5].Acronym);
            Assert.Equal("Other", resultList[6].Acronym);
        }

        [Fact]
        public void GetById_ShouldReturnCorrectBusinessArea_WhenExists()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetBusinessAreaRepository();

            // Act
            var result = mockRepository.Object.GetById(3);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("DES", result.Acronym);
            Assert.Equal("Digital Engagement Solutions", result.Name);
        }

        [Fact]
        public void GetById_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetBusinessAreaRepository();

            // Act
            var result = mockRepository.Object.GetById(999); // ID not in the list

            // Assert
            Assert.Null(result);
        }
    }
}
