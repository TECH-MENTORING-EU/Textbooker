using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Booker.Data;

namespace Booker.Data
{
    public class ChatMessage
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(500)]
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        [Required]
        [MaxLength(100)]
        public string DealId { get; set; } = string.Empty; // logical conversation grouping
    }
}
