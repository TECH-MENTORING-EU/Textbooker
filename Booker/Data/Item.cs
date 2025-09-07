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
        [Precision(10,2)]
        public required decimal Price { get; set; }
        public required DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public required string Description { get; set; }
        public required string State { get; set; }
        public required string Photo {  get; set; }
        public bool Reserved {  get; set; }
    }
}
