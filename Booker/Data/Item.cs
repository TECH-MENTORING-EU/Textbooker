using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booker.Data
{
    public class Item
    {
        public int Id { get; set; }
        public int BookId { get; init; } // Set by the database, not by the user, but needed for seeding
        public required Book Book { get; set; }
        public int UserId { get; init; } // Set by the database, not by the user, but needed for seeding
        public required User User { get; set; }
        [Precision(10, 2)]
        public required decimal Price { get; set; }
        public required DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public required string Description { get; set; }
        public required string State { get; set; }
        public required string Photo { get; set; }
        public bool IsVisible { get; set; } = true;
        public bool CanChangeVisibility { get; set; } = true;
        public bool Reserved {  get; set; }

        /// <summary>
        /// UTC timestamp of the moment the seller marked the item as reserved.
        /// Starts the transaction lifecycle: after 7 days the seller is asked
        /// whether the sale happened, after 30 days the item auto-completes as sold.
        /// </summary>
        public DateTime? ReservedAt { get; set; }

        /// <summary>
        /// True once the transaction completed: the seller confirmed the sale,
        /// or the 30-day auto-close window elapsed. Only sold items allow ratings.
        /// </summary>
        public bool IsSold { get; set; }

        /// <summary>UTC timestamp when the item was marked sold (manually or auto).</summary>
        public DateTime? SoldAt { get; set; }

        public ICollection<ItemView> Views { get; } = new HashSet<ItemView>();
    }
}
