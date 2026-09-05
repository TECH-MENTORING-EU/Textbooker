using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Booker.Authorization;

// ASP.NET Core only invokes authorization handlers resolved from DI, so the
// handler lives in its own typed class - a requirement implementing
// IAuthorizationHandler is never discovered and the policy could never pass.
public class AdminHiddenAuthorizationHandler : AuthorizationHandler<AdminHiddenAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdminHiddenAuthorizationRequirement requirement)
    {
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Fail with a bare 404 instead of the login/access-denied redirect so the
        // admin area's existence stays hidden; ConfigureApplicationCookie turns the
        // HideUnauthorized marker into a 404 without a redirect.
        if (context.Resource is AuthorizationFilterContext afc)
        {
            afc.HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            afc.HttpContext.Items["HideUnauthorized"] = true;
            afc.Result = new NotFoundResult();
        }
        else if (context.Resource is HttpContext httpContext)
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            httpContext.Items["HideUnauthorized"] = true;
        }

        context.Fail();
        return Task.CompletedTask;
    }
}
