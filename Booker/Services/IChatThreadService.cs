using Booker.Data;

namespace Booker.Services
{
    public interface IChatThreadService
    {
        Task<ChatThread?> GetByChannelIdAsync(string channelId, CancellationToken ct);
        Task<ChatThread> GetOrCreateAsync(int userAId, int userBId, CancellationToken ct);

        /// <summary>
        /// Finds or creates the conversation about a specific listing between the
        /// item's buyer-to-be and its seller. Threads are only ever created here —
        /// always anchored to an offer, never user-to-user "cold".
        /// </summary>
        Task<ChatThread> GetOrCreateForItemAsync(int requesterId, int itemId, CancellationToken ct);

        Task<IReadOnlyList<ChatThread>> GetThreadsForUserAsync(int userId, CancellationToken ct);
        Task UpdateLastMessageUtcAsync(string channelId, DateTime utcNow, CancellationToken ct);
    }
}
