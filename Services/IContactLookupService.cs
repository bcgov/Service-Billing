using Microsoft.Graph;

namespace Service_Billing.Services
{
    public interface IContactLookupService
    {
        Task<IEnumerable<User>> LookupAsync(string query);
    }
}
