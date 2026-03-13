namespace Engine.ECS.Components;

public struct HazardComponent : IComponent
{
    public float Damage;
    public float Range;

    public static HazardComponent Create(float damage = 10f, float range = 1.5f) => new()
    {
        Damage = damage,
        Range = range
    };
}
