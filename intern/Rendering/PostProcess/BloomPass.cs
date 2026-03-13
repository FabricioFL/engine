using Silk.NET.OpenGL;

namespace Engine.Rendering.PostProcess;

public sealed class BloomPass : IDisposable
{
    private readonly GL _gl;
    private readonly Shader _extractShader;
    private readonly Shader _blurShader;
    private readonly Shader _compositeShader;
    private readonly FrameBuffer _brightFbo;
    private readonly FrameBuffer _pingFbo;
    private readonly FrameBuffer _pongFbo;
    private readonly Mesh _screenQuad;

    public float Threshold { get; set; } = 0.8f;
    public float Intensity { get; set; } = 1.0f;
    public int BlurPasses { get; set; } = 4;

    public BloomPass(GL gl, uint width, uint height, Mesh screenQuad,
        Shader extractShader, Shader blurShader, Shader compositeShader)
    {
        _gl = gl;
        _screenQuad = screenQuad;
        _extractShader = extractShader;
        _blurShader = blurShader;
        _compositeShader = compositeShader;

        uint halfW = width / 2;
        uint halfH = height / 2;
        _brightFbo = new FrameBuffer(gl, halfW, halfH, false);
        _pingFbo = new FrameBuffer(gl, halfW, halfH, false);
        _pongFbo = new FrameBuffer(gl, halfW, halfH, false);
    }

    public void Apply(FrameBuffer sceneBuffer, FrameBuffer outputBuffer)
    {
        // Step 1: Extract bright pixels
        _brightFbo.Bind();
        _gl.Clear(ClearBufferMask.ColorBufferBit);
        _extractShader.Use();
        _extractShader.SetFloat("uThreshold", Threshold);
        sceneBuffer.BindColorTexture();
        _extractShader.SetInt("uScene", 0);
        _screenQuad.Draw();

        // Step 2: Gaussian blur ping-pong
        bool horizontal = true;
        FrameBuffer readFbo = _brightFbo;

        for (int i = 0; i < BlurPasses * 2; i++)
        {
            var writeFbo = horizontal ? _pingFbo : _pongFbo;
            writeFbo.Bind();
            _gl.Clear(ClearBufferMask.ColorBufferBit);

            _blurShader.Use();
            _blurShader.SetInt("uHorizontal", horizontal ? 1 : 0);
            readFbo.BindColorTexture();
            _blurShader.SetInt("uImage", 0);
            _screenQuad.Draw();

            readFbo = writeFbo;
            horizontal = !horizontal;
        }

        // Step 3: Composite bloom with scene
        outputBuffer.Bind();
        _gl.Clear(ClearBufferMask.ColorBufferBit);
        _compositeShader.Use();
        _compositeShader.SetFloat("uBloomIntensity", Intensity);
        sceneBuffer.BindColorTexture(TextureUnit.Texture0);
        _compositeShader.SetInt("uScene", 0);
        readFbo.BindColorTexture(TextureUnit.Texture1);
        _compositeShader.SetInt("uBloom", 1);
        _screenQuad.Draw();
    }

    public void Dispose()
    {
        _brightFbo.Dispose();
        _pingFbo.Dispose();
        _pongFbo.Dispose();
    }
}
