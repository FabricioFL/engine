using System.Numerics;

namespace Engine.Animation;

public struct Keyframe
{
    public float Time;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;

    public Keyframe(float time, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Time = time;
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }

    public static Keyframe Lerp(in Keyframe a, in Keyframe b, float t)
    {
        return new Keyframe(
            a.Time + (b.Time - a.Time) * t,
            Vector3.Lerp(a.Position, b.Position, t),
            Quaternion.Slerp(a.Rotation, b.Rotation, t),
            Vector3.Lerp(a.Scale, b.Scale, t)
        );
    }
}
