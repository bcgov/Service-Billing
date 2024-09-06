
using ServiceBillingUnitTests.Mocks;
using Xunit;

namespace ServiceBillingUnitTests.Repositories
{
    public class ClientAccountRepositoryTests
    {
        [Fact]
        public void GetAll_ShouldReturnAllClientAccounts_OrderedByName()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetClientAccountRepository();

            // Act
            var result = mockRepository.Object.GetAll();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(4, resultList.Count);
        }

        [Fact]
        public void GetClientAccount_ShouldReturnCorrectClientAccount_WhenExists()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetClientAccountRepository();

            // Act
            var result = mockRepository.Object.GetClientAccount(501);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(501, result.Id);
            Assert.Equal("IT Services", result.Name);
        }

        [Fact]
        public void GetClientAccount_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetClientAccountRepository();

            // Act
            var result = mockRepository.Object.GetClientAccount(999); // ID not in the list

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void SearchClientAccounts_ShouldReturnMatchingClientAccounts()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetClientAccountRepository();

            // Act
            var result = mockRepository.Object.SearchClientAccounts("IT Services");

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal("IT Services", resultList[0].Name);
        }

        [Fact]
        public void GetClientIdFromClientNumber_ShouldReturnCorrectClientId_WhenExists()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetClientAccountRepository();

            // Act
            var result = mockRepository.Object.GetClientIdFromClientNumber(23);

            // Assert
            Assert.Equal(501, result);
        }

        [Fact]
        public void GetClientIdFromClientNumber_ShouldReturnZero_WhenNotExists()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetClientAccountRepository();

            // Act
            var result = mockRepository.Object.GetClientIdFromClientNumber(999);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetAccountsByContactName_ShouldReturnCorrectAccounts()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetClientAccountRepository();

            // Act
            var result = mockRepository.Object.GetAccountsByContactName("Liu, Angela GCPE:EX");

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal("GCPE Marketing", resultList[0].Name);
        }

        [Fact]
        public void GetAccountsByContactName_ShouldReturnEmpty_WhenNoMatch()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetClientAccountRepository();

            // Act
            var result = mockRepository.Object.GetAccountsByContactName("Non-Existent Contact");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetInactiveAccounts_ShouldReturnInactiveAccounts()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetClientAccountRepository();

            // Act
            var result = mockRepository.Object.GetInactiveAccounts();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Single(resultList);
        }

        [Fact]
        public void GetAccountsByOrgId_ShouldReturnCorrectAccounts()
        {
            // Arrange
            var mockRepository = Mocks.RepositoryMocks.GetClientAccountRepository();

            // Act
            var result = mockRepository.Object.GetAccountsByOrgId(50);

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Single(resultList);
        }

        [Fact]
        public void GetAccountsByOrgId_ShouldReturnEmpty_WhenNoMatch()
        {
            // Arrange
            var mockRepository = RepositoryMocks.GetClientAccountRepository();

            // Act
            var result = mockRepository.Object.GetAccountsByOrgId(999);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
