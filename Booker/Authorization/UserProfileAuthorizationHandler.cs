using System;
using Booker.Data;
using Booker.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace Booker.Authorization;

public class UserProfileAuthorizationHandler
    : AuthorizationHandler<OperationAuthorizationRequirement, User>
{
    UserManager<User> _userManager;
    ILogger<ItemIsOwnerAuthorizationHandler> _logger;

    public UserProfileAuthorizationHandler(UserManager<User> userManager, ILogger<ItemIsOwnerAuthorizationHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        User resource)
    {
        if (context.User == null || resource == null)
        {
            return Task.CompletedTask;
        }

        if (resource.Id == _userManager.GetUserId(context.User).IntOrDefault())
        {
            context.Succeed(requirement);
        }

        if (requirement.Name == UserOperations.Read.Name
            && resource.IsVisible)
        {
            context.Succeed(requirement);
        }

        if (requirement.Name == UserOperations.ReadFavorites.Name
            && resource.IsVisible
            && resource.AreFavoritesPublic)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
