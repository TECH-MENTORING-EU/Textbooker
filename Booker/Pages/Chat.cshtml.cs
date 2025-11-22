using Booker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;

namespace Booker.Pages
{
    [Authorize]
    public class ChatModel : PageModel
    {
        private readonly IChatService _chatService;
        private readonly InMemoryChatStore _inMemoryChatStore;
        private readonly IChatThreadService _threadService;
        private readonly ILogger<ChatModel> _logger;

        public ChatModel(IChatService chatService, InMemoryChatStore inMemoryChatStore, IChatThreadService threadService, ILogger<ChatModel> logger)
        {
            _chatService = chatService;
            _inMemoryChatStore = inMemoryChatStore;
            _threadService = threadService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)] public string DealId { get; set; } = string.Empty; // used as ChannelId now
        public int CurrentUserId { get; private set; }
        public List<ChatMessageDto> Messages { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            CurrentUserId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(DealId))
            {
                Messages = new List<ChatMessageDto>();
                return Page();
            }

            if (DealId == "test-deal")
            {
                _inMemoryChatStore.SeedIfEmpty(DealId, new[]
                {
                    new ChatMessageDto(1, DealId, 1, "Karol", "Cze??! To jest testowy czat ??", DateTime.UtcNow.AddMinutes(-5)),
                    new ChatMessageDto(2, DealId, 2, "Ania", "Dzia?a – wiadomo?ci s? w pami?ci.", DateTime.UtcNow.AddMinutes(-4)),
                    new ChatMessageDto(3, DealId, 1, "Karol", "Wy?lij co? z drugiego konta.", DateTime.UtcNow.AddMinutes(-3))
                });
                Messages = _inMemoryChatStore.GetMessages(DealId).ToList();
                return Page();
            }

            var thread = await _threadService.GetByChannelIdAsync(DealId, cancellationToken);
            if (thread == null || (thread.UserAId != CurrentUserId && thread.UserBId != CurrentUserId))
            {
                _logger.LogWarning("Unauthorized access attempt to channel {ChannelId} by user {UserId}", DealId, CurrentUserId);
                return NotFound();
            }

            var msgs = await _chatService.GetMessagesAsync(DealId, 200, cancellationToken);
            Messages = msgs.ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostSendAsync(string dealId, string text, CancellationToken ct)
        {
            int userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(dealId)) return BadRequest();
            if (dealId != "test-deal")
            {
                var thread = await _threadService.GetByChannelIdAsync(dealId, ct);
                if (thread == null || (thread.UserAId != userId && thread.UserBId != userId)) return Unauthorized();
            }

            ChatMessageDto dto;
            if (dealId == "test-deal")
            {
                dto = _inMemoryChatStore.AddMessage(dealId, userId, User.Identity!.Name ?? $"U{userId}", text);
            }
            else
            {
                var result = await _chatService.AddMessageAsync(dealId, userId, text, ct);
                if (!result.Success) return Content($"<p class=\"error\">{System.Net.WebUtility.HtmlEncode(result.Error)}", "text/html");
                dto = result.Message!;
            }

            string fragment = RenderMessage(dto, userId);
            return Content(fragment, "text/html");
        }

        public async Task<IActionResult> OnGetSinceAsync(string dealId, int afterMessageId, CancellationToken ct)
        {
            int userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(dealId)) return Content(string.Empty, "text/html");
            if (dealId != "test-deal")
            {
                var thread = await _threadService.GetByChannelIdAsync(dealId, ct);
                if (thread == null || (thread.UserAId != userId && thread.UserBId != userId)) return Content(string.Empty, "text/html");
            }

            List<ChatMessageDto> msgs;
            if (dealId == "test-deal")
            {
                msgs = _inMemoryChatStore.GetMessages(dealId).Where(m => m.Id > afterMessageId).OrderBy(m => m.Id).ToList();
            }
            else
            {
                var all = await _chatService.GetMessagesAsync(dealId, 200, ct);
                msgs = all.Where(m => m.Id > afterMessageId).OrderBy(m => m.Id).ToList();
            }

            if (msgs.Count == 0) return Content(string.Empty, "text/html");
            var sb = new StringBuilder();
            foreach (var m in msgs)
            {
                sb.Append(RenderMessage(m, userId));
            }
            return Content(sb.ToString(), "text/html");
        }

        private static string RenderMessage(ChatMessageDto m, int currentUserId)
        {
            string cls = m.UserId == currentUserId ? "self" : "other";
            return $"<article class=\"msg {cls}\" data-msg-id=\"{m.Id}\"><header><strong>{System.Net.WebUtility.HtmlEncode(m.UserDisplayName)}</strong></header><p>{System.Net.WebUtility.HtmlEncode(m.Content)}</p><footer><time datetime=\"{m.CreatedUtc.ToString("O")}\">{m.CreatedUtc.ToLocalTime():HH:mm}</time></footer></article>";
        }

        private int GetCurrentUserId()
        {
            string? idStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idStr, out int id) ? id : -1;
        }
    }
}
