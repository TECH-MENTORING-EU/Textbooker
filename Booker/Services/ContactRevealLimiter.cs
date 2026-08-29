using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Booker.Services;

// RODO — zadanie 07: limit ujawnień danych kontaktowych liczony wyłącznie w pamięci procesu
// (IMemoryCache) — celowo bez żadnego trwałego zapisu, ponieważ audyt działań użytkowników
// (w odróżnieniu od audytu administratorskiego) nie jest tu wymagany. Licznik resetuje się
// przy restarcie aplikacji i jest per-instancja, jeśli aplikacja działa na wielu instancjach.
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
