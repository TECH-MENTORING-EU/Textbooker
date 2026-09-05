using Booker.Data;
using Booker.Services;
using Booker.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Booker.Pages
{
    /// <summary>
    /// Inbox of the user's conversations. Threads are only ever created from a
    /// listing page ("Napisz do sprzedającego"); this page only lists and opens them.
    /// </summary>
    [Authorize]
    public class MessagesModel(UserManager<User> userManager, IChatThreadService threadService) : PageModel
    {
        public List<ThreadVm> Threads { get; private set; } = new();

        public async Task OnGetAsync(CancellationToken ct)
        {
            var currentUserId = userManager.GetUserId(User).IntOrDefault();

            Threads = (await threadService.GetInboxAsync(currentUserId, ct))
                .Select(t => new ThreadVm(
                    t.ChannelId,
                    t.ItemTitle ?? t.DisplayName,
                    t.ItemTitle != null,
                    t.ItemPhoto,
                    t.DisplayName,
                    PolishTime.ToLocal(t.LastMessageUtc).ToString("yyyy-MM-dd HH:mm")))
                .ToList();
        }

        public record ThreadVm(string ChannelId, string Title, bool IsItemThread, string? ItemPhoto, string DisplayName, string LastMessageLocal);
    }
}
