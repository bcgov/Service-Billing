using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using Service_Billing.Extensions;

namespace Service_Billing.Filters
{

public class GroupAuthorizeActionFilter : IActionFilter
{
    private readonly string _groupId;

    public GroupAuthorizeActionFilter(string groupId)
    {
        _groupId = groupId;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User;

        // user is unauthorized if they're a ministry client
        var isNotAuthorized = user.IsMemberOfGroup(_groupId);

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
