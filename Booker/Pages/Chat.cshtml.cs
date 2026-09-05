using System.Text;
using Booker.Services;
using Booker.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Booker.Pages
{
    /// <summary>
    /// One conversation thread. Threads are reached from a listing or the inbox;
    /// both participants see the same transcript, refreshed by HTMX polling.
    /// </summary>
    [Authorize]
    public class ChatModel(UserManager<Data.User> userManager, IChatService chatService, IChatThreadService threadService, ILogger<ChatModel> logger) : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string DealId { get; set; } = string.Empty;

        public int CurrentUserId { get; private set; }
        public List<ChatMessageDto> Messages { get; private set; } = new();

        /// <summary>The sidebar: all of the user's conversations, newest first.</summary>
        public IReadOnlyList<ChatInboxEntry> Threads { get; private set; } = Array.Empty<ChatInboxEntry>();

        /// <summary>The active conversation's counterpart; null when no thread is open.</summary>
        public ChatInboxEntry? ActiveThread { get; private set; }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            CurrentUserId = userManager.GetUserId(User).IntOrDefault();
            Threads = await threadService.GetInboxAsync(CurrentUserId, cancellationToken);

            // Without a thread the chat page IS the inbox: the full thread
            // list renders in place, no separate /Messages page.
            if (string.IsNullOrWhiteSpace(DealId))
            {
                return Page();
            }

            ActiveThread = Threads.FirstOrDefault(t => t.ChannelId == DealId);

            if (ActiveThread == null || !await IsParticipantAsync(ct: cancellationToken))
            {
                logger.LogWarning("Unauthorized access attempt to channel {ChannelId} by user {UserId}", DealId, CurrentUserId);
                return NotFound();
            }

            var messages = await chatService.GetMessagesAsync(DealId, 200, cancellationToken);
            Messages = messages.ToList();
            // Rendering the conversation means the user has seen it up to now.
            await threadService.MarkThreadReadAsync(DealId, CurrentUserId, DateTime.UtcNow, cancellationToken);
            return Page();
        }

        public async Task<IActionResult> OnGetThreadAsync(string dealId, CancellationToken ct)
        {
            // Sidebar navigation swaps only this pane; the full page reload is
            // the no-JavaScript fallback (the sidebar links keep their href).
            DealId = dealId ?? string.Empty;
            CurrentUserId = userManager.GetUserId(User).IntOrDefault();
            Threads = await threadService.GetInboxAsync(CurrentUserId, ct);
            ActiveThread = Threads.FirstOrDefault(t => t.ChannelId == DealId);

            if (ActiveThread == null || !await IsParticipantAsync(ct: ct))
            {
                return NotFound();
            }

            var messages = await chatService.GetMessagesAsync(DealId, 200, ct);
            Messages = messages.ToList();
            await threadService.MarkThreadReadAsync(DealId, CurrentUserId, DateTime.UtcNow, ct);
            return Partial("_Conversation", this);
        }

        public async Task<IActionResult> OnPostSendAsync(string dealId, string text, CancellationToken ct)
        {
            var userId = userManager.GetUserId(User).IntOrDefault();
            // IsParticipantAsync reads CurrentUserId; handlers other than OnGetAsync
            // must set it themselves or the participant check compares against 0.
            CurrentUserId = userId;
            if (string.IsNullOrWhiteSpace(dealId))
            {
                return BadRequest();
            }

            if (!await IsParticipantAsync(dealId, ct))
            {
                return Unauthorized();
            }

            var result = await chatService.AddMessageAsync(dealId, userId, text ?? string.Empty, ct);
            if (!result.Success)
            {
                return Content(RenderNotice(result.Error ?? "Nie udało się wysłać wiadomości."), "text/html");
            }

            return Content(RenderMessage(result.Message!, userId), "text/html");
        }

        public async Task<IActionResult> OnGetSinceAsync(string dealId, int afterMessageId, CancellationToken ct)
        {
            var userId = userManager.GetUserId(User).IntOrDefault();
            CurrentUserId = userId;
            if (string.IsNullOrWhiteSpace(dealId) || !await IsParticipantAsync(dealId, ct))
            {
                return Content(string.Empty, "text/html");
            }

            var all = await chatService.GetMessagesAsync(dealId, 200, ct);
            var fresh = all.Where(m => m.Id > afterMessageId).OrderBy(m => m.Id).ToList();

            if (fresh.Count == 0)
            {
                return Content(string.Empty, "text/html");
            }

            var sb = new StringBuilder();
            foreach (var message in fresh)
            {
                sb.Append(RenderMessage(message, userId));
            }
            // New messages were delivered to an open conversation: seen.
            await threadService.MarkThreadReadAsync(dealId, userId, DateTime.UtcNow, ct);
            return Content(sb.ToString(), "text/html");
        }

        public string FormatLocalTime(DateTime utc) => PolishTime.ToLocal(utc).ToString("HH:mm");

        private async Task<bool> IsParticipantAsync(string? dealId = null, CancellationToken ct = default)
        {
            var channelId = dealId ?? DealId;
            var thread = await threadService.GetByChannelIdAsync(channelId, ct);
            return thread != null
                && (thread.UserAId == CurrentUserId || thread.UserBId == CurrentUserId);
        }

        private static string RenderMessage(ChatMessageDto m, int currentUserId)
        {
            var side = m.UserId == currentUserId ? "self" : "other";
            var escapedName = System.Net.WebUtility.HtmlEncode(m.UserDisplayName);
            var escapedContent = System.Net.WebUtility.HtmlEncode(m.Content);
            var utc = DateTime.SpecifyKind(m.CreatedUtc, DateTimeKind.Utc);
            var localTime = PolishTime.ToLocal(m.CreatedUtc);
            return $"<li class=\"msg {side}\" data-msg-id=\"{m.Id}\">" +
                   $"<div class=\"bubble\"><div class=\"meta\"><span class=\"user\">{escapedName}</span>" +
                   $"<time datetime=\"{utc:o}\">{localTime:HH:mm}</time></div>" +
                   $"<p class=\"content\">{escapedContent}</p></div></li>";
        }

        private static string RenderNotice(string error) =>
            // Signed as System so moderation feedback is not mistaken for a
            // reply from the other conversation participant.
            "<li class=\"msg system\" role=\"alert\">" +
            "<div class=\"system-note\"><span class=\"system-name\">System</span>" +
            $"<p class=\"content\">{System.Net.WebUtility.HtmlEncode(error)}</p></div></li>";
    }
}
