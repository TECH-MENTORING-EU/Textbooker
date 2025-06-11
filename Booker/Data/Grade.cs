namespace Booker.Data
{
    public class Grade
    {
        public required int Id { get; set; }
        public required string GradeNumber { get; set; }

        public virtual ICollection<Book> Books { get; set; } = new HashSet<Book>();
    }
}
