using Booker.Data;

namespace Booker.Services
{
    public interface IChatService
    {
        Task<IReadOnlyList<ChatMessageDto>> GetMessagesAsync(string dealId, int take, CancellationToken cancellationToken);
        Task<ChatMessageResult> AddMessageAsync(string dealId, int userId, string content, CancellationToken cancellationToken);
    }

    public record ChatMessageDto(int Id, string DealId, int UserId, string UserDisplayName, string Content, DateTime CreatedUtc);

    public record ChatMessageResult(bool Success, ChatMessageDto? Message, string? Error)
    {
        public static ChatMessageResult Failure(string error) => new ChatMessageResult(false, null, error);
        public static ChatMessageResult Ok(ChatMessageDto dto) => new ChatMessageResult(true, dto, null);
    }
}
