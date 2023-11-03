namespace Service_Billing.Graph
{
    using Azure.Core;
    using Azure.Identity;
    using Microsoft.Graph;
    using Microsoft.Identity.Client;
    using Microsoft.Identity.Web;
    using System.Net.Http.Headers;

    public class GraphHelper
    {
        private static string ClientId = "e6008dda-22e3-43b4-acf7-a953ac99b661";
        private static string Tenant = "6fdb5200-3d0d-4a8a-b036-d3685e359adc";
        public static IPublicClientApplication PublicClientApp;
        private static string[] scopes = { "user.read", "user.readbasic.all" };
        private static GraphServiceClient graphClient;

        public static async Task<string> GetAccessToken()
        {
            PublicClientApp = PublicClientApplicationBuilder.Create(ClientId)
                .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
                .WithAuthority(AzureCloudInstance.AzurePublic, Tenant)
                .Build();

            IEnumerable<IAccount> accounts = await PublicClientApp.GetAccountsAsync().ConfigureAwait(false);
            IAccount firstAccount = accounts.FirstOrDefault();
            var authResult = await PublicClientApp.AcquireTokenSilent(scopes, firstAccount)
                                                  .ExecuteAsync();

            authResult = await PublicClientApp.AcquireTokenInteractive(scopes)
                                      .ExecuteAsync();

            return authResult.AccessToken;
        }

        public static async Task<GraphServiceClient> GetGraphServiceClient()
        {
            var delegateAuthProvider = new DelegateAuthenticationProvider((requestMessage) =>
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", GetAccessToken().Result);

                return Task.FromResult(0);
            });
            var graphClient = new GraphServiceClient(delegateAuthProvider);

            return graphClient;
        }

        public async Task<User> GetMeAsync()
        {
            graphClient = GetGraphServiceClient().Result;

            var user = await graphClient.Me
                .Request()
                .GetAsync();

            return user;
        }
    }
}
