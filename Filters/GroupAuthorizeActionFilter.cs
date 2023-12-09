using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Service_Billing.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace Service_Billing.Filters
{
    public class GroupAuthorizeActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;
            var authorizationService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();

            var isNotAuthorized = user.IsMinistryClient(authorizationService);

            if (isNotAuthorized)
            {
                context.Result = new UnauthorizedResult();
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }

}
