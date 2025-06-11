using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booker.Data
{
    public class Item
    {
        public required int Id { get; set; }
        public required int BookId { get; set; }
        public Book Book { get; set; } = null!;
        public required int UserId { get; set; }
        public User User { get; set; } = null!;
        [Precision(10,2)]
        public required decimal Price { get; set; }
        public required DateTime DateTime { get; set; }
        public required string Description { get; set; }
        public required string State { get; set; }
        public required string Photo {  get; set; }
    }
}
