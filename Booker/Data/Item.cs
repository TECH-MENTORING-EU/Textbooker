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
        /// whether the sale happened, after 30 days the reservation auto-releases.
        /// </summary>
        public DateTime? ReservedAt { get; set; }

        /// <summary>
        /// True once the seller confirmed the transaction completed. Sold listings
        /// disappear from browsing but stay reachable by their direct link.
        /// </summary>
        public bool IsSold { get; set; }

        /// <summary>UTC timestamp when the seller confirmed the sale.</summary>
        public DateTime? SoldAt { get; set; }

        /// <summary>
        /// The buyer the seller named when confirming the sale. Only this user may
        /// rate the seller for this listing; null means the sale happened outside
        /// TextBooker (or the buyer is unknown), so nobody earns rating rights.
        /// </summary>
        public int? SoldToUserId { get; set; }

        public User? SoldToUser { get; set; }

        public ICollection<ItemView> Views { get; } = new HashSet<ItemView>();
    }
}
