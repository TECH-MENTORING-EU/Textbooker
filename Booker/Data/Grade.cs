namespace Booker.Data
{
    public class Grade : IEquatable<Grade>
    {
        public int Id { get; set; }
        public required string GradeNumber { get; set; }

        public ICollection<Book> Books { get; } = new HashSet<Book>();



        #region IEquatable

        public bool Equals(Grade? other)
        {
            if (other is null) return false;
            return Id == other.Id;
        }

        public override bool Equals(object? obj) => Equals(obj as Grade);
        public override int GetHashCode() => Id.GetHashCode();

        public static bool operator ==(Grade? left, Grade? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(Grade? left, Grade? right) => !(left == right);

        #endregion IEquatable
    }
}
