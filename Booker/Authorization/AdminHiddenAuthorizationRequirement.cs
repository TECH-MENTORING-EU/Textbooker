using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Booker.Authorization;

public class AdminHiddenAuthorizationRequirement : IAuthorizationRequirement, IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (context.User == null || !context.User.Identity!.IsAuthenticated
            || !context.User.IsInRole("Admin"))
        {
            if (context.Resource is AuthorizationFilterContext afc)
            {
                afc.HttpContext.Response.StatusCode = 404;
                afc.HttpContext.Items["HideUnauthorized"] = true;
                afc.Result = new NotFoundResult();
            }
            else if (context.Resource is HttpContext hc)
            {
                hc.Response.StatusCode = 404;
                hc.Items["HideUnauthorized"] = true;
            }
            context.Fail();
        }
        else
        {
            context.Succeed(this);
        }
        return Task.CompletedTask;
    }
}
