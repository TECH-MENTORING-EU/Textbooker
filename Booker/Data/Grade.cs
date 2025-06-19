namespace Booker.Data
{
    public class Grade
    {
        public int Id { get; set; }
        public required string GradeNumber { get; set; }

        public ICollection<Book> Books { get; } = new HashSet<Book>();
    }
}
