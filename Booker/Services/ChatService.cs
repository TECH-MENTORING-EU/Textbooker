using Booker.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Booker.Services
{
    public class ChatService : IChatService
    {
        private readonly DataContext _context;
        private readonly ILogger<ChatService> _logger;
        private readonly IChatThreadService _threadService; // added
        public ChatService(DataContext context, ILogger<ChatService> logger, IChatThreadService threadService)
        {
            _context = context;
            _logger = logger;
            _threadService = threadService; // added
        }

        public async Task<IReadOnlyList<ChatMessageDto>> GetMessagesAsync(string dealId, int take, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(dealId)) return Array.Empty<ChatMessageDto>();
            var messages = await _context.ChatMessages
                .AsNoTracking()
                .Where(m => m.DealId == dealId)
                .OrderByDescending(m => m.CreatedUtc)
                .Take(take)
                .OrderBy(m => m.CreatedUtc)
                .Select(m => new ChatMessageDto(m.Id, m.DealId, m.UserId, m.User.UserName ?? $"U{m.UserId}", m.Content, m.CreatedUtc))
                .ToListAsync(cancellationToken);
            return messages;
        }

        public async Task<ChatMessageResult> AddMessageAsync(string dealId, int userId, string content, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(dealId)) return ChatMessageResult.Failure("DealId required");
            if (string.IsNullOrWhiteSpace(content)) return ChatMessageResult.Failure("Content required");
            content = Sanitize(content);
            if (content.Length > 500) return ChatMessageResult.Failure("Too long");

            var user = await _context.Users.FindAsync([userId], cancellationToken);
            if (user == null) return ChatMessageResult.Failure("User not found");

            var entity = new ChatMessage
            {
                DealId = dealId,
                UserId = userId,
                Content = content,
                CreatedUtc = DateTime.UtcNow
            };
            _context.ChatMessages.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            // update thread last message timestamp if thread exists
            await _threadService.UpdateLastMessageUtcAsync(dealId, DateTime.UtcNow, cancellationToken);

            var dto = new ChatMessageDto(entity.Id, entity.DealId, entity.UserId, user.UserName ?? $"U{userId}", entity.Content, entity.CreatedUtc);
            return ChatMessageResult.Ok(dto);
        }

        private static string Sanitize(string input)
        {
            string trimmed = input.Trim();
            trimmed = Regex.Replace(trimmed, "\r?\n", " ");
            return trimmed;
        }
    }
}
