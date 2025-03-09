using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booker.Data
{
    public class Book
    {         
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public ICollection<Grade> Grades { get; set; }
        [Required]
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }
        [Required]
        public bool? Level { get; set; }
        public ICollection<Item> Items { get; set; }
    }
}
