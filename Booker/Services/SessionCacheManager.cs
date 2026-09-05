using System;
using System.Threading.Tasks;
using Booker.Data;
using Booker.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Booker.Services;

public class SessionCacheManager(
    SessionCacheStore store,
    UserManager<User> userManager,
    ILogger<SessionCacheManager> logger)
{
    public async Task<bool> CheckSession(HttpContext context)
    {
        var userId = userManager.GetUserId(context.User).IntOrDefault();
        if (userId <= 0)
        {
            return false;
        }

        if (store.Sessions.TryGetValue(userId, out var session))
        {
            // A cached invalid entry means an explicit InvalidateSessionAsync call;
            // it stays invalid until CleanupSessions drops it, so the invalidated
            // cookie is rejected even before the rotated security stamp is noticed.
            if (!session.Valid)
            {
                return false;
            }

            // Compare-and-swap refresh. Losing the swap must be told apart:
            // losing to a concurrent InvalidateSessionAsync (entry invalid or
            // gone) signs this request out so the next one cannot resurrect
            // the invalidated session, but losing to a concurrent refresh of
            // the same still-valid session is benign and must not sign the
            // legitimate user out.
            var refreshed = session with { LastActivity = DateTime.Now };
            if (store.Sessions.TryUpdate(userId, refreshed, session))
            {
                return true;
            }

            return store.Sessions.TryGetValue(userId, out var current) && current.Valid;
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.LockoutEnd > DateTimeOffset.Now)
        {
            return false;
        }

        // TryAdd loses when an invalidation landed while this request was
        // querying the database; trust whichever entry is in the store rather
        // than assuming the one we just built won.
        if (!store.Sessions.TryAdd(userId, new SessionCacheStore.SessionInfo(Valid: true, LastActivity: DateTime.Now)))
        {
            return store.Sessions.TryGetValue(userId, out var added) && added.Valid;
        }

        return true;
    }

    public async Task InvalidateSessionAsync(int userId)
    {
        // No LastActivity: an invalid entry is not user activity, and
        // WritebackSessions would otherwise record the lockout/deletion
        // moment as the user's last active time.
        store.Sessions[userId] = new SessionCacheStore.SessionInfo(Valid: false, LastActivity: null);
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            await userManager.UpdateSecurityStampAsync(user);
        }
        logger.LogInformation("Sesja użytkownika o ID {UserId} została unieważniona.", userId);
    }

    // Drops the cached session entry so the next request validates the user
    // against the database again; used after unlocking, where the cached invalid
    // entry would otherwise keep rejecting the user until the next cleanup pass.
    public void RemoveCachedSession(int userId)
    {
        store.Sessions.TryRemove(userId, out _);
    }

    public async Task WritebackSessions()
    {
        foreach (var (userId, session) in store.Sessions)
        {
            if (session.LastActivity.HasValue)
            {
                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user != null)
                {
                    user.LastActiveAt = session.LastActivity.Value;
                    await userManager.UpdateAsync(user);
                }
            }
        }
        logger.LogInformation("Sesje użytkowników zostały zapisane.");
    }

    public void CleanupSessions()
    {
        var now = DateTime.Now;
        var removed = 0;
        foreach (var (userId, session) in store.Sessions)
        {
            if (!session.Valid || session.LastActivity.HasValue && (now - session.LastActivity.Value).TotalMinutes > 5)
            {
                store.Sessions.TryRemove(userId, out _);
                removed++;
            }
        }

        logger.LogInformation("Wyczyszczono {Removed} nieaktywnych sesji użytkownika.", removed);
    }
}
