using Booker.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Booker.Services;

/// <summary>
/// RODO - Phase 3: Centralized sign-in eligibility check used everywhere Identity
/// evaluates <c>RequireConfirmedAccount</c> (e.g. <see cref="SignInManager{TUser}"/>
/// during password sign-in). The default <see cref="DefaultUserConfirmation{TUser}"/>
/// only checks <see cref="User.EmailConfirmed"/>, which lets a minor who confirmed
/// their own email sign in - and reach <c>[Authorize]</c> pages - before their
/// guardian has confirmed consent. This implementation additionally requires that
/// any pending guardian consent for the account has been confirmed.
/// </summary>
public class GuardianConsentUserConfirmation : IUserConfirmation<User>
{
    private readonly DataContext _context;

    public GuardianConsentUserConfirmation(DataContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<bool> IsConfirmedAsync(UserManager<User> manager, User user)
    {
        if (!await manager.IsEmailConfirmedAsync(user))
        {
            return false;
        }

        var consent = await _context.GuardianConsents
            .AsNoTracking()
            .FirstOrDefaultAsync(gc => gc.UserId == user.Id);

        // No consent record means the account never required one (self-declared 16+),
        // or it was already used/cleared. Only an unconfirmed consent blocks sign-in.
        return consent is null || consent.ConfirmedAtUtc.HasValue;
    }
}
