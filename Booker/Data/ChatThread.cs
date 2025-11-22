namespace Booker.Data
{
    public class ChatThread
    {
        public int Id { get; set; }
        public string ChannelId { get; set; } = string.Empty; // deterministic id
        public int UserAId { get; set; }
        public int UserBId { get; set; }
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime LastMessageUtc { get; set; } = DateTime.UtcNow;
    }
}
