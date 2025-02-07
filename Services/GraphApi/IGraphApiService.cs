using Microsoft.Identity.Client;
using Service_Billing.Models;

namespace Service_Billing.Services.GraphApi
{
    public interface IGraphApiService
    {
        public Task<GraphApiListResponse<GraphUser>> GetUsers(IConfidentialClientApplication cca);
        public Task<GraphUser> Me(string id, IConfidentialClientApplication cca);
        public Task<GraphApiListResponse<GraphUser>> GetUsersByDisplayName(string term, IConfidentialClientApplication cca);
        public Task<GraphUser> GetUserByDisplayName(string term, IConfidentialClientApplication cca);
    }
}
