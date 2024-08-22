
using Xunit;

namespace ServiceBillingUnitTests.Repositories
{
    public class FiscalPeriodRepositoryTests
    {
        [Fact]
        public void GetFiscalPeriodByString_ShouldReturnFiscalPeriod_WhenExists()
        {
            // Arrange
            var mockFiscalPeriodRepository = Mocks.RepositoryMocks.GetFiscalPeriodRepository();

            // Act
            var result = mockFiscalPeriodRepository.Object.GetFiscalPeriodByString("Fiscal 23/24 Quarter 3");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(259, result.Id);
        }

        [Fact]
        public void GetFiscalPeriodByString_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            var mockFiscalPeriodRepository = Mocks.RepositoryMocks.GetFiscalPeriodRepository();

            // Act
            var result = mockFiscalPeriodRepository.Object.GetFiscalPeriodByString("Fiscal 22/23 Q4");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetByFiscalQuarterString_ShouldReturnFiscalPeriod_WhenExists()
        {
            // Arrange
            var mockFiscalPeriodRepository = Mocks.RepositoryMocks.GetFiscalPeriodRepository();

            // Act
            var result = mockFiscalPeriodRepository.Object.GetByFiscalQuarterString("Fiscal 23/24 Quarter 3");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(259, result.Id);
        }

        [Fact]
        public void GetByFiscalQuarterString_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            var mockFiscalPeriodRepository = Mocks.RepositoryMocks.GetFiscalPeriodRepository();

            // Act
            var result = mockFiscalPeriodRepository.Object.GetByFiscalQuarterString("Fiscal 22/23 Q4");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetByFiscalQuarterString_ShouldReturnCorrectFiscalPeriod_WhenMultipleExist()
        {
            // Arrange
            var mockFiscalPeriodRepository = Mocks.RepositoryMocks.GetFiscalPeriodRepository();

            // Act
            var result = mockFiscalPeriodRepository.Object.GetByFiscalQuarterString("Fiscal 23/24 Quarter 4");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(260, result.Id);
        }

        [Fact]
        public void GetFiscalPeriodById_ShouldReturnCorrectFiscalPeriod_WhenExists()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetFiscalPeriodRepository();

            // Act
            var result = mockRepository.Object.GetFiscalPeriodById(259);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(259, result.Id);
            Assert.Equal("Fiscal 23/24 Quarter 3", result.Period);
        }

        [Fact]
        public void GetFiscalPeriodById_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetFiscalPeriodRepository();

            // Act
            var result = mockRepository.Object.GetFiscalPeriodById(999); // ID not in the list

            // Assert
            Assert.Null(result);
        }
    }
}
