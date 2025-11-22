using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Booker.Services
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly InMemoryChatStore _inMemoryChatStore; // in-memory for test
        private readonly IChatThreadService _threadService; // added
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IChatService chatService, InMemoryChatStore inMemoryChatStore, IChatThreadService threadService, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _inMemoryChatStore = inMemoryChatStore;
            _threadService = threadService; // added
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            string? channelId = Context.GetHttpContext()?.Request.Query["dealId"]; // dealId used as channel id
            if (!string.IsNullOrWhiteSpace(channelId))
            {
                int currentUserId = GetCurrentUserId();
                if (channelId == "test-deal")
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"deal-{channelId}");
                }
                else
                {
                    var thread = await _threadService.GetByChannelIdAsync(channelId, CancellationToken.None);
                    if (thread != null && (thread.UserAId == currentUserId || thread.UserBId == currentUserId))
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, $"deal-{channelId}");
                    }
                    else
                    {
                        _logger.LogWarning("User {UserId} attempted to join unauthorized channel {ChannelId}", currentUserId, channelId);
                    }
                }
            }
            await base.OnConnectedAsync();
        }

        public async Task SendMessage(string dealId, string content, CancellationToken cancellationToken = default)
        {
            if (!Context.User.Identity?.IsAuthenticated ?? true)
            {
                return;
            }

            int userId = GetCurrentUserId();

            string sanitized = Sanitize(content);
            if (string.IsNullOrEmpty(sanitized))
            {
                return;
            }

            ChatMessageDto? dto;
            if (dealId == "test-deal")
            {
                dto = _inMemoryChatStore.AddMessage(dealId, userId, Context.User.Identity!.Name ?? $"U{userId}", sanitized);
            }
            else
            {
                var thread = await _threadService.GetByChannelIdAsync(dealId, cancellationToken);
                if (thread == null || (thread.UserAId != userId && thread.UserBId != userId))
                {
                    _logger.LogWarning("User {UserId} attempted to send message to unauthorized channel {ChannelId}", userId, dealId);
                    return;
                }

                var result = await _chatService.AddMessageAsync(dealId, userId, sanitized, cancellationToken);
                if (!result.Success)
                {
                    _logger.LogWarning("Message send failed: {Error}", result.Error);
                    return;
                }
                dto = result.Message!;
            }

            await Clients.Group($"deal-{dealId}").SendAsync("MessageAdded", new
            {
                dto.Id,
                dto.DealId,
                dto.UserId,
                dto.UserDisplayName,
                dto.Content,
                CreatedUtc = dto.CreatedUtc.ToString("O")
            }, cancellationToken);
        }

        private int GetCurrentUserId()
        {
            string? userIdStr = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdStr, out int userId) ? userId : -1;
        }

        private static string Sanitize(string input)
        {
            string trimmed = input.Trim();
            if (trimmed.Length > 500) trimmed = trimmed[..500];
            trimmed = trimmed.Replace('\r', ' ').Replace('\n', ' ');
            while (trimmed.Contains("  ")) trimmed = trimmed.Replace("  ", " ");
            return trimmed;
        }
    }
}
