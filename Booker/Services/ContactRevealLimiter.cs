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

    public bool TryRegisterReveal(int userId)
    {
        var now = DateTime.Now;
        var limits = options.Value;

        var reveals = cache.GetOrCreate(CacheKey(userId), entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromDays(1);
            return new List<DateTime>();
        })!;

        lock (reveals)
        {
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
}
