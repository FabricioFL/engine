using System.Numerics;
using System.Runtime.InteropServices;
using Engine.ECS;
using Engine.ECS.Components;
using Silk.NET.OpenGL;

namespace Engine.Rendering.Lighting;

public sealed class LightManager : IDisposable
{
    private readonly GL _gl;
    private readonly uint _ubo;
    private const int MaxPointLights = 16;

    // std140 layout: DirLight(32) + Ambient+Count(16) + PointLights(32*16) = 560 bytes
    private const int UboSize = 32 + 16 + 32 * MaxPointLights;

    public Vector3 AmbientColor { get; set; } = new(0.15f, 0.15f, 0.15f);

    public LightManager(GL gl)
    {
        _gl = gl;

        _ubo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.UniformBuffer, _ubo);
        _gl.BufferData(BufferTargetARB.UniformBuffer, UboSize, ReadOnlySpan<byte>.Empty, BufferUsageARB.DynamicDraw);
        _gl.BindBufferBase(BufferTargetARB.UniformBuffer, 0, _ubo);
        _gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);
    }

    public unsafe void UpdateLights(EntityManager entities)
    {
        var lightPool = entities.GetPool<LightComponent>();
        var transformPool = entities.GetPool<TransformComponent>();

        var dirLight = new DirectionalLightData
        {
            Direction = new Vector3(0, -1, -0.5f),
            Color = Vector3.One,
            Intensity = 0
        };

        Span<PointLightData> pointLights = stackalloc PointLightData[MaxPointLights];
        int pointCount = 0;

        var lightSpan = lightPool.AsSpan();
        for (int i = 0; i < lightSpan.Length; i++)
        {
            ref var light = ref lightSpan[i];
            int entityId = lightPool.GetEntityAtIndex(i);

            if (light.Type == LightType.Directional)
            {
                dirLight.Direction = Vector3.Normalize(light.Direction);
                dirLight.Color = light.Color;
                dirLight.Intensity = light.Intensity;
            }
            else if (light.Type == LightType.Point && pointCount < MaxPointLights)
            {
                if (transformPool.Has(entityId))
                {
                    pointLights[pointCount] = new PointLightData
                    {
                        Position = transformPool.Get(entityId).Position,
                        Range = light.Range,
                        Color = light.Color,
                        Intensity = light.Intensity
                    };
                    pointCount++;
                }
            }
        }

        _gl.BindBuffer(BufferTargetARB.UniformBuffer, _ubo);

        // std140 layout matches shader:
        // offset 0:  vec3 dirDirection + float _pad0  (16 bytes)
        // offset 16: vec3 dirColor + float dirIntensity (16 bytes)
        // offset 32: vec3 ambientColor + int pointLightCount (16 bytes)
        // offset 48: PointLight[16] (32 bytes each)

        int offset = 0;

        // DirectionalLightData is already 32 bytes with correct padding
        _gl.BufferSubData(BufferTargetARB.UniformBuffer, offset, (nuint)sizeof(DirectionalLightData), in dirLight);
        offset += sizeof(DirectionalLightData); // 32

        // Ambient color (vec3, 12 bytes)
        var ambient = AmbientColor;
        _gl.BufferSubData(BufferTargetARB.UniformBuffer, offset, (nuint)sizeof(Vector3), in ambient);
        offset += 12;

        // Point light count (int, 4 bytes) - fills out the 16-byte block
        _gl.BufferSubData(BufferTargetARB.UniformBuffer, offset, sizeof(int), in pointCount);
        offset += 4; // now at 48

        // Point lights array
        if (pointCount > 0)
        {
            fixed (PointLightData* ptr = pointLights)
                _gl.BufferSubData(BufferTargetARB.UniformBuffer, offset,
                    (nuint)(sizeof(PointLightData) * pointCount), ptr);
        }

        _gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);
    }

    public void BindToShader(Shader shader, string blockName = "LightBlock")
    {
        uint index = _gl.GetUniformBlockIndex(shader.Handle, blockName);
        if (index != uint.MaxValue)
            _gl.UniformBlockBinding(shader.Handle, index, 0);
    }

    public void Dispose()
    {
        _gl.DeleteBuffer(_ubo);
    }
}
