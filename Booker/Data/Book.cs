using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booker.Data
{
    public class Book
    {         
        public int Id { get; set; }
        public required string Title { get; set; }
        public required ICollection<Grade> Grades { get; set; }
        public int SubjectId { get; init; } // Set by the database, not by the user, but needed for seeding
        public required Subject Subject { get; set; }
        public bool? Level { get; set; }
        public ICollection<Item> Items { get; } = new HashSet<Item>();
    }
}
