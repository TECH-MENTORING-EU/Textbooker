using Microsoft.AspNetCore.Authorization;

namespace Booker.Authorization;

/// <summary>
/// Marks a request as subject to the hidden-admin convention: admins pass,
/// everyone else gets a bare 404 (see <see cref="AdminHiddenAuthorizationHandler"/>).
/// </summary>
public class AdminHiddenAuthorizationRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// HttpContext.Items key the handler sets so the cookie redirect overrides
    /// in StartupUtilities return a bare 404 instead of a redirect.
    /// </summary>
    public const string HideUnauthorizedItemKey = "HideUnauthorized";
}
