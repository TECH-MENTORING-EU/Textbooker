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

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            CurrentUserId = userManager.GetUserId(User).IntOrDefault();

            if (string.IsNullOrWhiteSpace(DealId))
            {
                return Page();
            }

            if (!await IsParticipantAsync(ct: cancellationToken))
            {
                logger.LogWarning("Unauthorized access attempt to channel {ChannelId} by user {UserId}", DealId, CurrentUserId);
                return NotFound();
            }

            var messages = await chatService.GetMessagesAsync(DealId, 200, cancellationToken);
            Messages = messages.ToList();
            return Page();
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
            return Content(sb.ToString(), "text/html");
        }

        public string FormatLocalTime(DateTime utc) =>
            ConvertToLocal(utc).ToString("HH:mm");

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
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(utc, PolishTimeZone);
            return $"<li class=\"msg {side}\" data-msg-id=\"{m.Id}\">" +
                   $"<div class=\"bubble\"><div class=\"meta\"><span class=\"user\">{escapedName}</span>" +
                   $"<time datetime=\"{utc:o}\">{localTime:HH:mm}</time></div>" +
                   $"<p class=\"content\">{escapedContent}</p></div></li>";
        }

        private static string RenderNotice(string error) =>
            "<li class=\"msg other\" role=\"alert\"><div class=\"bubble\"><p class=\"content\">" +
            $"{System.Net.WebUtility.HtmlEncode(error)}</p></div></li>";

        private static DateTime ConvertToLocal(DateTime utc) =>
            // EF materializes DateTime with Kind=Unspecified; ConvertTime would
            // then interpret it in the machine zone instead of UTC.
            TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(utc, DateTimeKind.Utc), PolishTimeZone);

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
