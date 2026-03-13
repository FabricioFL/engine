using System.Numerics;

namespace Engine.ECS.Components;

public struct ColliderComponent : IComponent
{
    public ColliderShape Shape;
    public Vector3 Size;      // Box: half-extents; Sphere: X=radius; Capsule: X=radius, Y=length
    public Vector3 Offset;
    public bool IsTrigger;

    public static ColliderComponent Box(Vector3 halfExtents, bool isTrigger = false) => new()
    {
        Shape = ColliderShape.Box,
        Size = halfExtents,
        Offset = Vector3.Zero,
        IsTrigger = isTrigger
    };

    public static ColliderComponent Sphere(float radius, bool isTrigger = false) => new()
    {
        Shape = ColliderShape.Sphere,
        Size = new Vector3(radius, 0, 0),
        Offset = Vector3.Zero,
        IsTrigger = isTrigger
    };

    public static ColliderComponent Capsule(float radius, float length, bool isTrigger = false) => new()
    {
        Shape = ColliderShape.Capsule,
        Size = new Vector3(radius, length, 0),
        Offset = Vector3.Zero,
        IsTrigger = isTrigger
    };
}

public enum ColliderShape
{
    Box = 0,
    Sphere = 1,
    Capsule = 2,
    Mesh = 3
}
