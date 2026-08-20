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

            // Compare-and-swap refresh: when InvalidateSessionAsync replaces the
            // entry concurrently, this write loses and the next request is
            // rejected instead of resurrecting the invalidated session.
            store.Sessions.TryUpdate(userId, session with { LastActivity = DateTime.Now }, session);
            return true;
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.Now)
        {
            return false;
        }

        store.Sessions.TryAdd(userId, new SessionCacheStore.SessionInfo(Valid: true, LastActivity: DateTime.Now));
        return true;
    }

    public async Task InvalidateSessionAsync(int userId)
    {
        store.Sessions[userId] = new SessionCacheStore.SessionInfo(Valid: false, LastActivity: DateTime.Now);
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            await userManager.UpdateSecurityStampAsync(user);
        }
        logger.LogInformation($"Sesja użytkownika o ID {userId} została unieważniona.");
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

        logger.LogInformation($"Wyczyszczono {removed} nieaktywnych sesji użytkownika.");
    }
}
