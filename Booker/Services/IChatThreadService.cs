using Booker.Data;

namespace Booker.Services
{
    /// <summary>
    /// One row of the user's inbox: which listing it is about and who the
    /// conversation is with, ready for the sidebar on the chat page.
    /// ItemPhoto is the raw first-photo path; views resolve it to a URL.
    /// </summary>
    public record ChatInboxEntry(
        string ChannelId,
        int OtherUserId,
        string DisplayName,
        int? ItemId,
        string? ItemTitle,
        string? ItemPhoto,
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

        /// <summary>
        /// How many of the user's threads hold a message they have not seen:
        /// the thread's last message is newer than their read stamp.
        /// </summary>
        Task<int> GetUnreadCountAsync(int currentUserId, CancellationToken ct);

        /// <summary>Records that the user has seen the thread up to now.</summary>
        Task MarkThreadReadAsync(string channelId, int userId, DateTime utcNow, CancellationToken ct);

        /// <summary>
        /// Moves the thread's last-message stamp and marks the SENDER as
        /// caught up, so own messages never count as unread for their author.
        /// </summary>
        Task UpdateLastMessageUtcAsync(string channelId, int senderId, DateTime utcNow, CancellationToken ct);
    }
}
