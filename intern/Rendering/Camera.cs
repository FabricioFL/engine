using System.Numerics;

namespace Engine.Rendering;

public sealed class Camera
{
    public Vector3 Position { get; set; } = new(0, 2, 5);
    public Vector3 Front { get; private set; } = -Vector3.UnitZ;
    public Vector3 Up { get; private set; } = Vector3.UnitY;
    public Vector3 Right { get; private set; } = Vector3.UnitX;

    public float Yaw { get; set; } = -90f;
    public float Pitch { get; set; } = -20f;
    public float Fov { get; set; } = 45f;
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 1000f;
    public float AspectRatio { get; set; } = 800f / 600f;

    public bool IsOrthographic { get; set; }
    public float OrthoSize { get; set; } = 10f;

    public Matrix4x4 ViewMatrix { get; private set; }
    public Matrix4x4 ProjectionMatrix { get; private set; }
    public Matrix4x4 ViewProjectionMatrix { get; private set; }

    public void UpdateMatrices()
    {
        UpdateVectors();

        ViewMatrix = Matrix4x4.CreateLookAt(Position, Position + Front, Up);

        ProjectionMatrix = IsOrthographic
            ? Matrix4x4.CreateOrthographic(OrthoSize * AspectRatio, OrthoSize, NearPlane, FarPlane)
            : Matrix4x4.CreatePerspectiveFieldOfView(
                Fov * MathF.PI / 180f, AspectRatio, NearPlane, FarPlane);

        ViewProjectionMatrix = ViewMatrix * ProjectionMatrix;
    }

    private void UpdateVectors()
    {
        float yawRad = Yaw * MathF.PI / 180f;
        float pitchRad = Pitch * MathF.PI / 180f;

        Front = Vector3.Normalize(new Vector3(
            MathF.Cos(yawRad) * MathF.Cos(pitchRad),
            MathF.Sin(pitchRad),
            MathF.Sin(yawRad) * MathF.Cos(pitchRad)
        ));

        Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
        Up = Vector3.Normalize(Vector3.Cross(Right, Front));
    }
}
