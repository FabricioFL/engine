using Silk.NET.OpenGL;

namespace Engine.Rendering.PostProcess;

public sealed class FrameBuffer : IDisposable
{
    private readonly GL _gl;
    private readonly uint _fbo;
    private readonly uint _colorTexture;
    private readonly uint _depthTexture;
    private readonly uint _width;
    private readonly uint _height;

    public uint ColorTexture => _colorTexture;
    public uint DepthTexture => _depthTexture;

    public FrameBuffer(GL gl, uint width, uint height, bool hasDepth = true)
    {
        _gl = gl;
        _width = width;
        _height = height;

        _fbo = _gl.GenFramebuffer();
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);

        // Color attachment
        _colorTexture = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, _colorTexture);
        _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba16f,
            width, height, 0, PixelFormat.Rgba, PixelType.Float, ReadOnlySpan<byte>.Empty);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, _colorTexture, 0);

        // Depth attachment
        if (hasDepth)
        {
            _depthTexture = _gl.GenTexture();
            _gl.BindTexture(TextureTarget.Texture2D, _depthTexture);
            _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.DepthComponent24,
                width, height, 0, PixelFormat.DepthComponent, PixelType.Float, ReadOnlySpan<byte>.Empty);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
                TextureTarget.Texture2D, _depthTexture, 0);
        }

        var status = _gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != GLEnum.FramebufferComplete)
            throw new Exception($"Framebuffer incomplete: {status}");

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void Bind()
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        _gl.Viewport(0, 0, _width, _height);
    }

    public static void Unbind(GL gl)
    {
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void BindColorTexture(TextureUnit unit = TextureUnit.Texture0)
    {
        _gl.ActiveTexture(unit);
        _gl.BindTexture(TextureTarget.Texture2D, _colorTexture);
    }

    public void BindDepthTexture(TextureUnit unit = TextureUnit.Texture1)
    {
        _gl.ActiveTexture(unit);
        _gl.BindTexture(TextureTarget.Texture2D, _depthTexture);
    }

    public void Dispose()
    {
        _gl.DeleteFramebuffer(_fbo);
        _gl.DeleteTexture(_colorTexture);
        if (_depthTexture != 0) _gl.DeleteTexture(_depthTexture);
    }
}
