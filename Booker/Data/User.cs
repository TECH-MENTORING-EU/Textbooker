using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Booker.Data
{
    public class User : IdentityUser<int>
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastActiveAt { get; set; }
        public bool IsVisible { get; set; } = true;
        
        /// <summary>
        /// Foreign key to the School table. Nullable - users without assigned school have null.
        /// </summary>
        public int? SchoolId { get; set; }
        
        /// <summary>
        /// Navigation property to the School entity
        /// </summary>
        public School? School { get; set; }
        
        public string? Photo { get; set; }
        public ICollection<Item> Items { get; } = new HashSet<Item>();
        public ICollection<Item> Favorites { get; } = new HashSet<Item>();
        public bool AreFavoritesPublic { get; set; } = false;
        public bool DisplayEmail { get; set; } = true;
        public bool DisplayWhatsapp { get; set; } = false;
        public string? FbMessenger { get; set; }
        public string? Instagram { get; set; }
    }
}
