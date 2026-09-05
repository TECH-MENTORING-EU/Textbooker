namespace Booker.Data
{
    public class ChatThread
    {
        public int Id { get; set; }
        public string ChannelId { get; set; } = string.Empty; // deterministic id
        public int UserAId { get; set; }
        public int UserBId { get; set; }

        /// <summary>
        /// The listing this conversation is about. Created from the offer page,
        /// never from a bare user-to-user start. Null only for legacy threads.
        /// </summary>
        public int? ItemId { get; set; }
        public Item? Item { get; set; }

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime LastMessageUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Per-participant read stamps: a message newer than the reader's
        /// stamp (or than creation for someone who never opened the thread)
        /// counts as unread for exactly that participant.
        /// </summary>
        public DateTime? UserAReadUtc { get; set; }
        public DateTime? UserBReadUtc { get; set; }
    }
}
