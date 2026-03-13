using System.Numerics;

namespace Engine.ECS.Components;

public struct LightComponent : IComponent
{
    public LightType Type;
    public Vector3 Color;
    public float Intensity;
    public float Range;
    public Vector3 Direction; // For directional lights

    public static LightComponent Directional(Vector3 direction, Vector3 color, float intensity) => new()
    {
        Type = LightType.Directional,
        Color = color,
        Intensity = intensity,
        Direction = Vector3.Normalize(direction)
    };

    public static LightComponent Point(Vector3 color, float intensity, float range) => new()
    {
        Type = LightType.Point,
        Color = color,
        Intensity = intensity,
        Range = range
    };
}

public enum LightType
{
    Directional = 0,
    Point = 1
}
