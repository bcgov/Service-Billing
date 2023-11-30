using System.Security.Claims;

namespace Service_Billing.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool IsMinistryClient(this ClaimsPrincipal user)
        {
            var rvl = user?.Claims?.Any(c => c.Type == "groups" && c.Value == "") ?? false;
            return rvl;
        }
    }
}
