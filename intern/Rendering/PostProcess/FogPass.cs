using System.Numerics;
using Silk.NET.OpenGL;

namespace Engine.Rendering.PostProcess;

public sealed class FogPass
{
    private readonly GL _gl;
    private readonly Shader _fogShader;
    private readonly Mesh _screenQuad;

    public Vector3 FogColor { get; set; } = new(0.7f, 0.7f, 0.8f);
    public float FogStart { get; set; } = 20f;
    public float FogEnd { get; set; } = 100f;
    public float FogDensity { get; set; } = 0.02f;
    public bool Enabled { get; set; } = false;

    public FogPass(GL gl, Mesh screenQuad, Shader fogShader)
    {
        _gl = gl;
        _screenQuad = screenQuad;
        _fogShader = fogShader;
    }

    public void Apply(FrameBuffer inputBuffer, FrameBuffer outputBuffer, Camera camera)
    {
        if (!Enabled) return;

        outputBuffer.Bind();
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        _fogShader.Use();
        _fogShader.SetVector3("uFogColor", FogColor);
        _fogShader.SetFloat("uFogStart", FogStart);
        _fogShader.SetFloat("uFogEnd", FogEnd);
        _fogShader.SetFloat("uFogDensity", FogDensity);
        _fogShader.SetFloat("uNearPlane", camera.NearPlane);
        _fogShader.SetFloat("uFarPlane", camera.FarPlane);

        inputBuffer.BindColorTexture(TextureUnit.Texture0);
        _fogShader.SetInt("uScene", 0);
        inputBuffer.BindDepthTexture(TextureUnit.Texture1);
        _fogShader.SetInt("uDepth", 1);

        _screenQuad.Draw();
    }
}
