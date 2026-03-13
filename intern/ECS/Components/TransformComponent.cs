using System.Numerics;

namespace Engine.ECS.Components;

public struct TransformComponent : IComponent
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;
    public Matrix4x4 LocalToWorld;
    public bool IsDirty;

    public static TransformComponent Default => new()
    {
        Position = Vector3.Zero,
        Rotation = Quaternion.Identity,
        Scale = Vector3.One,
        LocalToWorld = Matrix4x4.Identity,
        IsDirty = true
    };

    public void UpdateMatrix()
    {
        if (!IsDirty) return;

        LocalToWorld = Matrix4x4.CreateScale(Scale) *
                       Matrix4x4.CreateFromQuaternion(Rotation) *
                       Matrix4x4.CreateTranslation(Position);
        IsDirty = false;
    }
}
