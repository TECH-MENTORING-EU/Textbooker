using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booker.Data
{
    public class GuardianConsent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [StringLength(256)]
        public string? GuardianEmail { get; set; }

        [Required]
        [StringLength(64)]
        public string TokenHash { get; set; } = null!;

        [Required]
        public DateTime RequestedAtUtc { get; set; }

        [Required]
        public DateTime ExpiresAtUtc { get; set; }

        public DateTime? ConfirmedAtUtc { get; set; }

        [StringLength(45)]
        public string? ConfirmationIpAddress { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
