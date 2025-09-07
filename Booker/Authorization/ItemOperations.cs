using System;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Booker.Authorization;

public static class ItemOperations
{
    public static OperationAuthorizationRequirement Create =
        new OperationAuthorizationRequirement { Name = Constants.CreateOperationName };
    public static OperationAuthorizationRequirement Read =
        new OperationAuthorizationRequirement { Name = Constants.ReadOperationName };
    public static OperationAuthorizationRequirement Update =
        new OperationAuthorizationRequirement { Name = Constants.UpdateOperationName };
    public static OperationAuthorizationRequirement Delete =
        new OperationAuthorizationRequirement { Name = Constants.DeleteOperationName };
}

public static class Constants
{
    public const string CreateOperationName = "Create";
    public const string ReadOperationName = "Read";
    public const string UpdateOperationName = "Update";
    public const string DeleteOperationName = "Delete";
}
