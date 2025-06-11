using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booker.Data
{
    public class Book
    {         
        public required int Id { get; set; }
        public required string Title { get; set; }
        public ICollection<Grade> Grades { get; set; } = new List<Grade>();
        public required int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;
        public bool? Level { get; set; }
        public ICollection<Item> Items { get; set; } = new HashSet<Item>();
    }
}
