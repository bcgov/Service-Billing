using Microsoft.Graph;
using Newtonsoft.Json;

namespace Service_Billing.Services
{
    public class ContactLookupService : IContactLookupService
    {
        public async Task<IEnumerable<User>> LookupAsync(string query)
        {
            using (var client = new HttpClient())
            {
                var endPoint = "../../ClientAccount/SearchForContact?query=" + query;
                var json = await client.GetStringAsync(endPoint);
                return JsonConvert.DeserializeObject<List<User>>(json);
            }
        }
    }
}
