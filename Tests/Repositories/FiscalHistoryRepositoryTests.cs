
using Xunit;

namespace ServiceBillingUnitTests.Repositories
{
    public class FiscalHistoryRepositoryTests
    {
        [Fact]
        public void All_ShouldReturnAllFiscalHistories()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetFiscalHistoryRepository();

            // Act
            var result = mockRepository.Object.All;

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(6, resultList.Count); // Assert that there are 6 records as per the mock data
        }

        [Fact]
        public void GetFiscalHistoriesByChargeId_ShouldReturnCorrectFiscalHistories()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetFiscalHistoryRepository();

            // Act
            var result = mockRepository.Object.GetFiscalHistoriesByChargeId(11612);

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Single(resultList); // Expecting only one record for BillId 11612
            Assert.Equal(4244, resultList[0].Id); // Assert that the ID of the returned record is 4244
        }

        [Fact]
        public void GetFiscalHistoriesByChargeId_ShouldReturnEmpty_WhenNoMatch()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetFiscalHistoryRepository();

            // Act
            var result = mockRepository.Object.GetFiscalHistoriesByChargeId(99999); // BillId not in mock data

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // Expecting no records
        }

        [Fact]
        public void GetFiscalHistoryByFiscalPeriodId_ShouldReturnCorrectFiscalHistories()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetFiscalHistoryRepository();

            // Act
            var result = mockRepository.Object.GetFiscalHistoryByFiscalPeriodId(259);

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(6, resultList.Count); // Expecting 6 records for PeriodId 259
        }

        [Fact]
        public void GetFiscalHistoryByFiscalPeriodId_ShouldReturnEmpty_WhenNoMatch()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetFiscalHistoryRepository();

            // Act
            var result = mockRepository.Object.GetFiscalHistoryByFiscalPeriodId(99999); // PeriodId not in mock data

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // Expecting no records
        }

        [Fact]
        public void GetFiscalHistoryById_ShouldReturnCorrectFiscalHistory_WhenExists()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetFiscalHistoryRepository();

            // Act
            var result = mockRepository.Object.GetFiscalHistoryById(4244);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4244, result.Id); // Assert that the ID of the returned record is 4244
            Assert.Equal(11612, result.BillId); // Verify the associated BillId
            Assert.Equal(259, result.PeriodId); // Verify the associated PeriodId
        }

        [Fact]
        public void GetFiscalHistoryById_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetFiscalHistoryRepository();

            // Act
            var result = mockRepository.Object.GetFiscalHistoryById(99999); // Id not in mock data

            // Assert
            Assert.Null(result); // Expecting no record (null) since ID doesn't exist
        }
    }
}
