namespace Engine.ECS.Components;

public struct RigidbodyComponent : IComponent
{
    public int BodyHandle;    // BepuPhysics body handle
    public float Mass;
    public bool IsKinematic;
    public bool UseGravity;

    public static RigidbodyComponent Dynamic(float mass = 1.0f) => new()
    {
        BodyHandle = -1,
        Mass = mass,
        IsKinematic = false,
        UseGravity = true
    };

    public static RigidbodyComponent Kinematic() => new()
    {
        BodyHandle = -1,
        Mass = 0,
        IsKinematic = true,
        UseGravity = false
    };
}
