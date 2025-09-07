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
        // Administrators can do anything (for now).
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
