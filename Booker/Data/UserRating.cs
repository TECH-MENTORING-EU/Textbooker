using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booker.Data
{
    public class UserRating
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReviewerId { get; set; }
        
        [ForeignKey("ReviewerId")]
        public User Reviewer { get; set; } = null!;

        [Required]
        public int RevieweeId { get; set; }
        
        [ForeignKey("RevieweeId")]
        public User Reviewee { get; set; } = null!;

        [Range(1, 5)]
        public int RatingValue { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
