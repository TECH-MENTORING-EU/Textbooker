namespace Booker.Data
{
    public class Subject : IEquatable<Subject>
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        #region IEquatable

        public bool Equals(Subject? other)
        {
            if (other is null) return false;
            return Id == other.Id;
        }
        public override bool Equals(object? obj) => Equals(obj as Subject);
        public override int GetHashCode() => Id.GetHashCode();

        public static bool operator ==(Subject? left, Subject? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(Subject? left, Subject? right) => !(left == right);

        #endregion IEquatable
    }
}
