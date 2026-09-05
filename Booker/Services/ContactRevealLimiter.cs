using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Booker.Services;

// RODO - task 07: the contact-reveal limit is counted purely in process memory (IMemoryCache) -
// deliberately without any persistent storage, since an audit trail of user actions (as opposed
// to an administrative audit trail) isn't required here. The counter resets on app restart and
// is per-instance if the app ever runs on multiple instances.
public class ContactRevealLimiter(IMemoryCache cache, IOptions<ContactRevealLimitOptions> options)
{
    private static string CacheKey(int userId) => $"contact-reveal-times:{userId}";
    private static string RejectionWarnedCacheKey(int userId) => $"contact-reveal-warned:{userId}";

    // Per-user locks guard cache initialization: IMemoryCache.GetOrCreate does not serialize
    // concurrent factories, so two first-requests for the same user could otherwise each create
    // and lock a different list, silently discarding one and letting a user exceed the limit.
    private readonly System.Collections.Concurrent.ConcurrentDictionary<int, object> _userLocks = new();

    public bool TryRegisterReveal(int userId)
    {
        var now = DateTime.Now;
        var limits = options.Value;

        var userLock = _userLocks.GetOrAdd(userId, static _ => new object());

        lock (userLock)
        {
            var reveals = cache.GetOrCreate(CacheKey(userId), entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromDays(1);
                return new List<DateTime>();
            })!;

            reveals.RemoveAll(t => t < now.AddDays(-1));

            var revealsLastHour = reveals.Count(t => t >= now.AddHours(-1));
            if (revealsLastHour >= limits.PerHour || reveals.Count >= limits.PerDay)
            {
                return false;
            }

            reveals.Add(now);
            return true;
        }
    }

    // An authenticated client can hit the reveal endpoint an unbounded number of times after
    // the limit is reached; without this, every single rejected request would write a warning
    // log line. Only the first rejection in a rolling window is logged per user.
    public bool ShouldLogRejection(int userId)
    {
        var key = RejectionWarnedCacheKey(userId);
        var userLock = _userLocks.GetOrAdd(userId, static _ => new object());

        lock (userLock)
        {
            if (cache.TryGetValue(key, out _))
            {
                return false;
            }

            cache.Set(key, true, TimeSpan.FromHours(1));
            return true;
        }
    }
}
