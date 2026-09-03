using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Booker.Pages
{
    /// <summary>
    /// Inbox of the user's conversations. Threads are only ever created from a
    /// listing page ("Napisz do sprzedającego") — this page only lists and opens them.
    /// </summary>
    [Authorize]
    public class MessagesModel : PageModel
    {
        private readonly IChatThreadService _threadService;
        private readonly DataContext _ctx;
        public List<ThreadVm> Threads { get; private set; } = new();

        public MessagesModel(IChatThreadService threadService, DataContext ctx)
        {
            _threadService = threadService;
            _ctx = ctx;
        }

        public async Task OnGetAsync(CancellationToken ct)
        {
            int currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var threads = await _threadService.GetThreadsForUserAsync(currentUserId, ct);
            Threads = threads.Select(t =>
            {
                int otherId = t.UserAId == currentUserId ? t.UserBId : t.UserAId;
                string otherName = _ctx.Users.Where(u => u.Id == otherId).Select(u => u.UserName!).First();
                return new ThreadVm(
                    t.ChannelId,
                    otherName,
                    t.LastMessageUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));
            }).ToList();
        }

        public record ThreadVm(string ChannelId, string DisplayName, string LastMessageLocal);
    }
}
