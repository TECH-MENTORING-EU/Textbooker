using Booker.Messenger.Models;

namespace Booker.Messenger.Services
{
    public interface IMessengerChatService
    {
        Task<IReadOnlyList<ChatConversation>> GetConversationsAsync(int loggedUserId, CancellationToken ct);
        Task<ChatMessagesPage> GetMessagesAsync(int conversationId, int loggedUserId, DateTime? olderThan, int weeksSpan, CancellationToken ct);
        Task<ChatConversationMessage> SendMessageAsync(int conversationId, int loggedUserId, string content, CancellationToken ct);
    }
}
