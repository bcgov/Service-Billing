using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Service_Billing.Extensions;

namespace Service_Billing.Filters
{

public class GroupAuthorizeActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User;
        var isNotAuthorized = user.IsMinistryClient();

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
