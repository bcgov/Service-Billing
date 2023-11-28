using System.Security.Claims;

namespace Service_Billing.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool IsMemberOfGroup(this ClaimsPrincipal user, string groupName)
        {
            var groupClaim = user?.Claims.FirstOrDefault(c => c.Type == "groups");

            if (groupClaim != null)
            {
                var groupList = groupClaim.Value.Split(',');
                return groupList.Contains(groupName);
            }

            return false;
        }
    }
}
