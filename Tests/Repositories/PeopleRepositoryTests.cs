
using Xunit;

namespace ServiceBillingUnitTests.Repositories
{
    public class PeopleRepositoryTests
    {
        [Fact]
        public void AllPeople_ReturnsAllPeople()
        {
            var mockPeopleRepository = Mocks.RepositoryMocks.GetPeopleRepository();

            var result = mockPeopleRepository.Object.AllPeople.ToList();

            Assert.Single(result);
        }
    }
}
