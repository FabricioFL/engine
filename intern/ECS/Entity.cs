namespace Engine.ECS;

public readonly struct Entity : IEquatable<Entity>
{
    public readonly int Id;
    public readonly int Generation;

    public Entity(int id, int generation)
    {
        Id = id;
        Generation = generation;
    }

    public bool IsValid => Id >= 0;
    public static Entity Invalid => new(-1, 0);

    public bool Equals(Entity other) => Id == other.Id && Generation == other.Generation;
    public override bool Equals(object? obj) => obj is Entity e && Equals(e);
    public override int GetHashCode() => HashCode.Combine(Id, Generation);
    public static bool operator ==(Entity a, Entity b) => a.Equals(b);
    public static bool operator !=(Entity a, Entity b) => !a.Equals(b);
    public override string ToString() => $"Entity({Id}:{Generation})";
}
