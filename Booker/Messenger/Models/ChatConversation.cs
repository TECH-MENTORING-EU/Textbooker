namespace Booker.Messenger.Models
{
    public class ChatConversation
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string LastMessage { get; set; } = string.Empty;
        public DateTime LastMessageUtc { get; set; }
    }

    public class ChatConversationMessage
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; }
    }

    public class ChatMessagesPage
    {
        public List<ChatConversationMessage> Messages { get; set; } = new();
        public bool HasOlderMessages { get; set; }
        public DateTime? OldestMessageDate { get; set; }
    }
}
