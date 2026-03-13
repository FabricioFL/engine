using System.Numerics;
using Silk.NET.OpenGL;

namespace Engine.Rendering.Effects;

public sealed class RainRenderer : IDisposable
{
    private readonly GL _gl;
    private readonly Shader _shader;
    private readonly uint _vao;
    private readonly uint _vbo;

    private const int MaxDrops = 800;
    private const int FloatsPerDrop = 6; // 2 endpoints x 3 components (pos)
    private readonly float[] _dropData = new float[MaxDrops * FloatsPerDrop];
    private readonly float[] _dropY = new float[MaxDrops];
    private readonly float[] _dropX = new float[MaxDrops];
    private readonly float[] _dropZ = new float[MaxDrops];
    private readonly float[] _dropSpeed = new float[MaxDrops];

    private readonly Random _rng = new();
    public bool Enabled { get; set; } = true;
    public float Intensity { get; set; } = 1.0f;
    public Vector3 PlayerPosition { get; set; }

    public RainRenderer(GL gl, Shader shader)
    {
        _gl = gl;
        _shader = shader;

        // Initialize drops
        for (int i = 0; i < MaxDrops; i++)
        {
            ResetDrop(i, true);
        }

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(_dropData.Length * sizeof(float)),
            ReadOnlySpan<byte>.Empty, BufferUsageARB.DynamicDraw);

        // Position only (vec3)
        _gl.EnableVertexAttribArray(0);
        unsafe { _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*)0); }

        _gl.BindVertexArray(0);
    }

    private void ResetDrop(int i, bool randomY)
    {
        float range = 25f;
        _dropX[i] = PlayerPosition.X + ((float)_rng.NextDouble() - 0.5f) * range * 2f;
        _dropZ[i] = PlayerPosition.Z + ((float)_rng.NextDouble() - 0.5f) * range * 2f;
        _dropY[i] = randomY ? (float)_rng.NextDouble() * 20f + 5f : 20f + (float)_rng.NextDouble() * 5f;
        _dropSpeed[i] = 12f + (float)_rng.NextDouble() * 8f;
    }

    public void Update(float deltaTime)
    {
        if (!Enabled) return;

        for (int i = 0; i < MaxDrops; i++)
        {
            _dropY[i] -= _dropSpeed[i] * deltaTime;

            if (_dropY[i] < 0)
                ResetDrop(i, false);

            int baseIdx = i * FloatsPerDrop;
            // Top of drop
            _dropData[baseIdx] = _dropX[i];
            _dropData[baseIdx + 1] = _dropY[i];
            _dropData[baseIdx + 2] = _dropZ[i];
            // Bottom of drop (slightly lower)
            _dropData[baseIdx + 3] = _dropX[i] + 0.02f;
            _dropData[baseIdx + 4] = _dropY[i] - 0.4f;
            _dropData[baseIdx + 5] = _dropZ[i];
        }
    }

    public unsafe void Render(Camera camera)
    {
        if (!Enabled) return;

        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _shader.Use();
        _shader.SetMatrix4("uView", camera.ViewMatrix);
        _shader.SetMatrix4("uProjection", camera.ProjectionMatrix);
        _shader.SetMatrix4("uModel", Matrix4x4.Identity);
        _shader.SetVector4("uColor", new Vector4(0.6f, 0.65f, 0.8f, 0.35f * Intensity));

        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        fixed (float* ptr = _dropData)
        {
            _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0,
                (nuint)(_dropData.Length * sizeof(float)), ptr);
        }

        _gl.DrawArrays(PrimitiveType.Lines, 0, (uint)(MaxDrops * 2));

        _gl.BindVertexArray(0);
        _gl.Disable(EnableCap.Blend);
    }

    public void Dispose()
    {
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteBuffer(_vbo);
    }
}
