using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Booker.Pages
{
    [Authorize]
    public class MessagesModel : PageModel
    {
        private readonly IChatThreadService _threadService;
        private readonly DataContext _ctx;
        public List<ThreadVm> Threads { get; private set; } = new();
        public List<User> OtherUsers { get; private set; } = new();

        public MessagesModel(IChatThreadService threadService, DataContext ctx)
        {
            _threadService = threadService;
            _ctx = ctx;
        }

        public async Task OnGetAsync(CancellationToken ct)
        {
            int currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)! .Value);

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

            OtherUsers = await _ctx.Users
                .Where(u => u.Id != currentUserId)
                .OrderBy(u => u.UserName)
                .ToListAsync(ct);
        }

        public async Task<IActionResult> OnGetUserOptionsAsync(string? q, CancellationToken ct)
        {
            int currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)! .Value);
            IQueryable<User> query = _ctx.Users.Where(u => u.Id != currentUserId);
            if (!string.IsNullOrWhiteSpace(q))
            {
                string term = q.Trim();
                query = query.Where(u => u.UserName!.Contains(term));
            }
            var users = await query
                .OrderBy(u => u.UserName)
                .Take(500)
                .Select(u => new { u.Id, u.UserName })
                .ToListAsync(ct);
            var sb = new StringBuilder();
            sb.Append("<option value=\"\">-- Wybierz u?ytkownika --</option>");
            foreach (var u in users)
            {
                sb.Append($"<option value=\"{u.Id}\">{System.Net.WebUtility.HtmlEncode(u.UserName)}</option>");
            }
            return Content(sb.ToString(), "text/html");
        }

        public async Task<IActionResult> OnPostStartAsync(int otherUserId, CancellationToken ct)
        {
            int currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)! .Value);
            if (otherUserId == currentUserId) return BadRequest();
            var thread = await _threadService.GetOrCreateAsync(currentUserId, otherUserId, ct);
            // htmx redirect to chat page with created/found channel id
            Response.Headers["HX-Redirect"] = $"/Chat/{thread.ChannelId}";
            return new EmptyResult();
        }

        public record ThreadVm(string ChannelId, string DisplayName, string LastMessageLocal);
    }
}
