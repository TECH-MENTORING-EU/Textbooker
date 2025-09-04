using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Booker.Authorization;

public class AdminAuthorizationHandler
                : AuthorizationHandler<OperationAuthorizationRequirement, object>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        object resource)
    {
        if (context.User == null || !context.User.Identity!.IsAuthenticated)
        {
            return Task.CompletedTask;
        }

        // Administrators can do anything (for now).
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
