using System.Collections.Concurrent;

namespace Booker.Services;

// Singleton shared state behind the scoped SessionCacheManager: every request scope
// and the MaintenanceService background worker mutate this same table concurrently.
public sealed class SessionCacheStore
{
    public sealed record SessionInfo(bool Valid = false, DateTime? LastActivity = null);

    public ConcurrentDictionary<int, SessionInfo> Sessions { get; } = new();
}
