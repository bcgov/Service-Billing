using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Service_Billing.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool IsMinistryClient(this ClaimsPrincipal user, IAuthorizationService authorizationService) =>
            authorizationService.AuthorizeAsync(user, "RequireUserRole").Result.Succeeded;
    }
}
