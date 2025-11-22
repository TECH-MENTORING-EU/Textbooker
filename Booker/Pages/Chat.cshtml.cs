using Booker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Booker.Pages
{
    [Authorize]
    public class ChatModel : PageModel
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatModel> _logger;
        public ChatModel(IChatService chatService, ILogger<ChatModel> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)] public string DealId { get; set; } = string.Empty;
        public List<ChatMessageDto> Messages { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(DealId))
            {
                return NotFound();
            }
            var msgs = await _chatService.GetMessagesAsync(DealId, 50, cancellationToken);
            Messages = msgs.ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostAddMessageAsync(string content, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(DealId)) return BadRequest();
            int userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var result = await _chatService.AddMessageAsync(DealId, userId, content, cancellationToken);
            if (!result.Success)
            {
                Response.StatusCode = 400;
                return Content($"<li class=\"error\">{result.Error}</li>", "text/html");
            }
            var html = $"<li><strong>{System.Net.WebUtility.HtmlEncode(result.Message!.UserDisplayName)}:</strong> {System.Net.WebUtility.HtmlEncode(result.Message.Content)}</li>";
            return Content(html, "text/html");
        }
    }
}
