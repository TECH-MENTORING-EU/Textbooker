using System.ComponentModel.DataAnnotations;

namespace Booker.Data
{
    /// <summary>
    /// Represents a school in the system. Each school has its own set of users and data.
    /// </summary>
    public class School : IEquatable<School>
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }
        
        /// <summary>
        /// Email domain associated with this school (e.g., "hogwart.edu.pl").
        /// Used for automatic school assignment during registration.
        /// Multiple domains can be stored as comma-separated values.
        /// </summary>
        [MaxLength(500)]
        public string? EmailDomain { get; set; }
        
        /// <summary>
        /// Navigation property for users belonging to this school
        /// </summary>
        public ICollection<User> Users { get; } = new HashSet<User>();

        #region IEquatable

        public bool Equals(School? other)
        {
            if (other is null) return false;
            return Id == other.Id;
        }

        public override bool Equals(object? obj) => Equals(obj as School);
        public override int GetHashCode() => Id.GetHashCode();

        public static bool operator ==(School? left, School? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(School? left, School? right) => !(left == right);

        #endregion IEquatable
    }
}
