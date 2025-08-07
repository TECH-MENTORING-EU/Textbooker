using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Booker.Data
{
    public class Book : IEquatable<Book>
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required ICollection<Grade> Grades { get; set; }
        public int SubjectId { get; init; } // Set by the database, not by the user, but needed for seeding
        public required Subject Subject { get; set; }
        public bool? Level { get; set; }
        public ICollection<Item> Items { get; } = new HashSet<Item>();



        #region IEquatable

        public bool Equals(Book? other)
        {
            if (other is null) return false;
            return Id == other.Id;
        }

        public override bool Equals(object? obj) => Equals(obj as Book);
        public override int GetHashCode() => Id.GetHashCode();

        public static bool operator ==(Book? left, Book? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(Book? left, Book? right) => !(left == right);
        
        #endregion IEquatable
    }
}
