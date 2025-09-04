using System;

namespace Booker.Data;

public class Level
{
    public int Id { get; set; }
    public required string Name { get; set; }

    #region IEquatable

    public bool Equals(Level? other)
    {
        if (other is null) return false;
        return Id == other.Id;
    }
    public override bool Equals(object? obj) => Equals(obj as Level);
    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Level? left, Level? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }
    
    public static bool operator !=(Level? left, Level? right) => !(left == right);

    #endregion IEquatable
}
