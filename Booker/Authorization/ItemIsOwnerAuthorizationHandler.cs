using System;
using Booker.Data;
using Booker.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace Booker.Authorization;

public class ItemIsOwnerAuthorizationHandler
        : AuthorizationHandler<OperationAuthorizationRequirement, Item>
{
    UserManager<User> _userManager;

    public ItemIsOwnerAuthorizationHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Item resource)
    {
        if (context.User == null || resource == null)
        {
            return Task.CompletedTask;
        }
        if (requirement.Name != Constants.CreateOperationName &&
            requirement.Name != Constants.ReadOperationName &&
            requirement.Name != Constants.UpdateOperationName &&
            requirement.Name != Constants.DeleteOperationName)
        {
            return Task.CompletedTask;
        }

        if (resource.User.Id == _userManager.GetUserId(context.User).IntOrDefault())
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
