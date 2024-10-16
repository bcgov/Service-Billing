using Microsoft.Identity.Client;
using Service_Billing.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Service_Billing.Services.GraphApi
{
    public class GraphApiService : IGraphApiService
    {
        private readonly HttpClient _httpClient;

        public GraphApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        //public async Task AccessGraphApi()
        //{
        //    var cca = ConfidentialClientApplicationBuilder
        //        .Create(_configuration.GetSection("AzureAd")["ClientId"])
        //        .WithClientSecret(_configuration.GetSection("AzureAd")["ClientSecret"])
        //        .WithAuthority(new Uri($"https://login.microsoftonline.com/{_configuration.GetSection("AzureAd")["TenantId"]}"))
        //        .Build();

        //    var users = await GetUsers(cca);
        //    var id = "39f54195-55ba-4f8c-a5dc-93579fc090cc";
        //    var me = await Me(id, cca);

        //    //foreach (var user in users?.Value ?? new List<GraphUser>())
        //    //{
        //    //    Console.WriteLine($"User: {user?.DisplayName}");
        //    //}
        //}

        public async Task<GraphApiListResponse<GraphUser>> GetUsers(IConfidentialClientApplication cca)
        {
            var emptyResponse = new GraphApiListResponse<GraphUser>();

            try
            {
                var result = await cca.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" })
                                      .ExecuteAsync();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

                var response = await _httpClient.GetAsync("https://graph.microsoft.com/v1.0/users");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return emptyResponse;
                }

                return await response.Content.ReadFromJsonAsync<GraphApiListResponse<GraphUser>>(
                    new JsonSerializerOptions(defaults: JsonSerializerDefaults.Web)
                ) ?? emptyResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetUsers method: {ex.Message}");
                return emptyResponse;
            }
        }

        public async Task<GraphUser> Me(string id, IConfidentialClientApplication cca)
        {
            var emptyResponse = new GraphUser();

            try
            {
                var result = await cca.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" })
                                      .ExecuteAsync();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

                var response = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return emptyResponse;
                }

                var json = await response.Content.ReadAsStringAsync();
                var rvl = JsonSerializer.Deserialize<GraphUser>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
                });

                return rvl ?? emptyResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Me method: {ex.Message}");
                return emptyResponse;
            }
        }

        public async Task<GraphApiListResponse<GraphUser>> GetUsersByDisplayName(string term, IConfidentialClientApplication cca)
        {
            var emptyResponse = new GraphApiListResponse<GraphUser>();

            try
            {
                var result = await cca.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" })
                                      .ExecuteAsync();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

                var url = $"https://graph.microsoft.com/v1.0/users?$filter=startswith(displayName, '{Uri.EscapeDataString(term)}')" +
                $"or startswith(givenName, '{Uri.EscapeDataString(term)}')" +
                $"&$select=displayName,id";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return emptyResponse;
                }

                var json = await response.Content.ReadAsStringAsync();
                var rvl = await response.Content.ReadFromJsonAsync<GraphApiListResponse<GraphUser>>(
                    new JsonSerializerOptions(defaults: JsonSerializerDefaults.Web)
                ) ?? emptyResponse;

                return rvl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetUsersByDisplayName method: {ex.Message}");
                return emptyResponse;
            }
        }

        public async Task<GraphUser> GetUserByDisplayName(string displayName, IConfidentialClientApplication cca)
        {
            var emptyResponse = new GraphUser();

            try
            {
                var result = await cca.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" })
                                      .ExecuteAsync();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

                var url = $"https://graph.microsoft.com/v1.0/users?$filter=startswith(displayName, '{Uri.EscapeDataString(displayName)}')&$top=1&$select=id";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return emptyResponse;
                }

                var json = await response.Content.ReadAsStringAsync();
                var user = await response.Content.ReadFromJsonAsync<GraphUser>(
                    new JsonSerializerOptions(defaults: JsonSerializerDefaults.Web)
                );

                return user ?? emptyResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetUserByDisplayName method: {ex.Message}");
                return emptyResponse;
            }
        }
    }
}
