namespace EqualityOverride
{
    public class Person : IEquatable<Person>
    {
        public required string FirstName { get; init; }
        public required string LastName { get; init; }

        public bool Equals(Person? other)
        => other is not null
           && FirstName == other.FirstName
           && LastName == other.LastName;

        public override bool Equals(object? obj)
            => obj is Person other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(FirstName, LastName);

        public static bool operator ==(Person? left, Person? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(Person? left, Person? right)
            => !(left == right);
    }
}
