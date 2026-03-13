using System.Numerics;
using System.Runtime.InteropServices;

namespace Engine.Rendering.Lighting;

[StructLayout(LayoutKind.Sequential)]
public struct DirectionalLightData
{
    public Vector3 Direction;
    public float _pad0;
    public Vector3 Color;
    public float Intensity;
}

[StructLayout(LayoutKind.Sequential)]
public struct PointLightData
{
    public Vector3 Position;
    public float Range;
    public Vector3 Color;
    public float Intensity;
}

[StructLayout(LayoutKind.Sequential)]
public struct LightBlock
{
    public DirectionalLightData DirectionalLight;
    public int PointLightCount;
    public Vector3 AmbientColor;
    // PointLights are uploaded separately due to array layout
}
