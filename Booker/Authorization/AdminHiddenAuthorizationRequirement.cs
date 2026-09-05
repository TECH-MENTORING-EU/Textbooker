using Microsoft.AspNetCore.Authorization;

namespace Booker.Authorization;

/// <summary>
/// Marks a request as subject to the hidden-admin convention: admins pass,
/// everyone else gets a bare 404 (see <see cref="AdminHiddenAuthorizationHandler"/>).
/// </summary>
public class AdminHiddenAuthorizationRequirement : IAuthorizationRequirement
{
}
