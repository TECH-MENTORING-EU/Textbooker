using Booker.Data;

namespace Booker.Services
{
    /// <summary>
    /// One row of the user's inbox: who the conversation is with and which
    /// listing it is about, ready for the sidebar on the chat page.
    /// </summary>
    public record ChatInboxEntry(
        string ChannelId,
        int OtherUserId,
        string DisplayName,
        int? ItemId,
        string? ItemTitle,
        DateTime LastMessageUtc);

    public interface IChatThreadService
    {
        Task<ChatThread?> GetByChannelIdAsync(string channelId, CancellationToken ct);

        /// <summary>
        /// Finds or creates the conversation about a specific listing between the
        /// item's buyer-to-be and its seller. Threads are only ever created here,
        /// always anchored to an offer, never user-to-user "cold".
        /// </summary>
        Task<ChatThread> GetOrCreateForItemAsync(int requesterId, int itemId, CancellationToken ct);

        Task<IReadOnlyList<ChatThread>> GetThreadsForUserAsync(int userId, CancellationToken ct);

        /// <summary>
        /// The user's threads ordered by recency, enriched with the other
        /// participant's name and the listing title for the sidebar.
        /// </summary>
        Task<IReadOnlyList<ChatInboxEntry>> GetInboxAsync(int currentUserId, CancellationToken ct);

        Task UpdateLastMessageUtcAsync(string channelId, DateTime utcNow, CancellationToken ct);
    }
}
