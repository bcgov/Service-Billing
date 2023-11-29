using System.Security.Claims;

namespace Service_Billing.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool IsMinistryClient(this ClaimsPrincipal user)
        {
            var rvl = user?.Claims?.Any(c => c.Type == "groups" && c.Value == "9e2cea8d-0dc8-4d4f-9bc9-d1930932d335") ?? false;
            return rvl;
        }
    }
}
