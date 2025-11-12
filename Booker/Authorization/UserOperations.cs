using System;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Booker.Authorization;

public class UserOperations
{
    public static OperationAuthorizationRequirement Read =
        new OperationAuthorizationRequirement { Name = nameof(Read) };
    public static OperationAuthorizationRequirement ReadFavorites =
        new OperationAuthorizationRequirement { Name = nameof(ReadFavorites) };
}
