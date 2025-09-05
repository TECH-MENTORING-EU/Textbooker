using System;
using System.Threading.Tasks;
using Booker.Data;
using Booker.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Booker.Services;

public class SessionCacheManager
{
    private readonly IMemoryCache _cache;
    private readonly UserManager<User> _userManager;
    private record struct SessionInfo(bool Valid = false, DateTime? LastActivity = null);
    private Dictionary<int, SessionInfo> _sessions;

    public SessionCacheManager(IMemoryCache cache, UserManager<User> userManager)
    {
        _cache = cache;
        _userManager = userManager;
        if (!_cache.TryGetValue("Sessions", out Dictionary<int, SessionInfo>? sessions))
        {
            sessions = new Dictionary<int, SessionInfo>();
            _cache.Set("Sessions", sessions);
        }
        _sessions = sessions!;
    }

    public async Task<bool> CheckSession(HttpContext context)
    {
        var userId = _userManager.GetUserId(context.User).IntOrDefault();
        var session = _sessions.GetValueOrDefault(userId, new SessionInfo());

        if (!session.Valid)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.Now)
            {
                return false;
            }
            session.Valid = true;
        }

        session.LastActivity = DateTime.Now;
        _sessions[userId] = session;
        _cache.Set("Sessions", _sessions);
        return session.Valid;
    }

    public void InvalidateSession(int userId)
    {
        var session = _sessions.GetValueOrDefault(userId, new SessionInfo());
        session.Valid = false;
        session.LastActivity = DateTime.Now;
        _sessions[userId] = session;
        _cache.Set("Sessions", _sessions);
    }

    public async Task WritebackSessions()
    {
        foreach (var (userId, session) in _sessions)
        {
            if (session.LastActivity.HasValue)
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user != null)
                {
                    user.LastActiveAt = session.LastActivity.Value;
                    await _userManager.UpdateAsync(user);
                }
            }
        }
    }

    public void CleanupSessions()
    {
        var now = DateTime.Now;
        var toRemove = _sessions
            .Where(kv => !kv.Value.Valid || kv.Value.LastActivity.HasValue && (now - kv.Value.LastActivity.Value).TotalMinutes > 5)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var userId in toRemove)
        {
            _sessions.Remove(userId);
        }

        _cache.Set("Sessions", _sessions);
    }
}
