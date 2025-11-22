using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Booker.Services
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            string? dealId = Context.GetHttpContext()?.Request.Query["dealId"];
            if (!string.IsNullOrWhiteSpace(dealId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"deal-{dealId}");
            }
            await base.OnConnectedAsync();
        }

        public async Task SendMessage(string dealId, string content, CancellationToken cancellationToken = default)
        {
            if (!Context.User.Identity?.IsAuthenticated ?? true)
            {
                return;
            }

            string? userIdStr = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return;
            }

            var result = await _chatService.AddMessageAsync(dealId, userId, content, cancellationToken);
            if (!result.Success)
            {
                _logger.LogWarning("Message send failed: {Error}", result.Error);
                return;
            }

            await Clients.Group($"deal-{dealId}").SendAsync("MessageAdded", new
            {
                result.Message!.Id,
                result.Message.DealId,
                result.Message.UserDisplayName,
                result.Message.Content,
                CreatedUtc = result.Message.CreatedUtc.ToString("O")
            }, cancellationToken);
        }
    }
}
