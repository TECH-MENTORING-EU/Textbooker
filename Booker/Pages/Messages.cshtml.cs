using Booker.Data;
using Booker.Services;
using Booker.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Booker.Pages
{
    /// <summary>
    /// Inbox of the user's conversations. Threads are only ever created from a
    /// listing page ("Napisz do sprzedającego"); this page only lists and opens them.
    /// </summary>
    [Authorize]
    public class MessagesModel(UserManager<User> userManager, IChatThreadService threadService, DataContext ctx) : PageModel
    {
        public List<ThreadVm> Threads { get; private set; } = new();

        public async Task OnGetAsync(CancellationToken ct)
        {
            var currentUserId = userManager.GetUserId(User).IntOrDefault();

            var threads = await threadService.GetThreadsForUserAsync(currentUserId, ct);

            var otherUserIds = threads
                .Select(t => t.UserAId == currentUserId ? t.UserBId : t.UserAId)
                .Distinct()
                .ToList();
            var displayNames = await ctx.Users
                .Where(u => otherUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName ?? $"U{u.Id}", ct);

            Threads = threads
                .Select(t =>
                {
                    var otherId = t.UserAId == currentUserId ? t.UserBId : t.UserAId;
                    return new ThreadVm(
                        t.ChannelId,
                        displayNames.GetValueOrDefault(otherId, "Konto usunięte"),
                        TimeZoneInfo.ConvertTime(t.LastMessageUtc, PolishTimeZone).ToString("yyyy-MM-dd HH:mm"));
                })
                .ToList();
        }

        public record ThreadVm(string ChannelId, string DisplayName, string LastMessageLocal);

        private static readonly TimeZoneInfo PolishTimeZone = CreatePolishTimeZone();

        private static TimeZoneInfo CreatePolishTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");
            }
            // Without ICU, Windows only knows its own zone id.
            catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            }
        }
    }
}
