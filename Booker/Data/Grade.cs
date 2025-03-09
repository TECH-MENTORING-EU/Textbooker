namespace Booker.Data
{
    public class Grade
    {
        public int Id { get; set; }
        public string GradeNumber { get; set; }

        public virtual ICollection<Book> Books { get; set; }
    }
}
