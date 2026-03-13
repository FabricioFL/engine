using System.Numerics;
using Engine.UI.Markup;
using Engine.UI.Runtime;
using Silk.NET.OpenGL;
using Shader = Engine.Rendering.Shader;

namespace Engine.UI.Rendering;

public sealed class UiRenderer : IDisposable
{
    private readonly GL _gl;
    private readonly Shader _shader;
    private readonly uint _vao;
    private readonly uint _vbo;
    private readonly uint _ebo;
    private Matrix4x4 _projection;
    private FontAtlas? _fontAtlas;

    // Dynamic text quad VBO
    private readonly uint _textVao;
    private readonly uint _textVbo;
    private readonly uint _textEbo;

    public UiRenderer(GL gl, Shader shader)
    {
        _gl = gl;
        _shader = shader;

        // Create a unit quad (0,0) to (1,1)
        float[] vertices =
        {
            // pos          // uv
            0f, 0f, 0f,    0f, 0f,
            1f, 0f, 0f,    1f, 0f,
            1f, 1f, 0f,    1f, 1f,
            0f, 1f, 0f,    0f, 1f,
        };
        uint[] indices = { 0, 1, 2, 2, 3, 0 };

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        unsafe
        {
            fixed (float* v = vertices)
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);
        }

        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        unsafe
        {
            fixed (uint* i = indices)
                _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);
        }

        // Position (vec3)
        _gl.EnableVertexAttribArray(0);
        unsafe { _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)0); }

        // TexCoord (vec2)
        _gl.EnableVertexAttribArray(1);
        unsafe { _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float))); }

        _gl.BindVertexArray(0);

        // Text quad VAO - dynamic UVs per character
        _textVao = _gl.GenVertexArray();
        _gl.BindVertexArray(_textVao);

        _textVbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _textVbo);
        _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(20 * sizeof(float)), ReadOnlySpan<byte>.Empty, BufferUsageARB.DynamicDraw);

        _textEbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _textEbo);
        unsafe
        {
            fixed (uint* i = indices)
                _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);
        }

        _gl.EnableVertexAttribArray(0);
        unsafe { _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)0); }
        _gl.EnableVertexAttribArray(1);
        unsafe { _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float))); }

        _gl.BindVertexArray(0);

        // Create font atlas
        _fontAtlas = new FontAtlas(gl);
    }

    public void UpdateProjection(float screenWidth, float screenHeight)
    {
        _projection = Matrix4x4.CreateOrthographicOffCenter(0, screenWidth, screenHeight, 0, -1, 1);
    }

    public void Render(UiManager uiManager)
    {
        _gl.Disable(EnableCap.DepthTest);
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _shader.Use();
        _shader.SetMatrix4("uProjection", _projection);
        _shader.SetInt("uHasTexture", 0);

        foreach (var view in uiManager.GetMountedViews())
        {
            RenderNode(view.RootNode);
        }

        _gl.Disable(EnableCap.Blend);
        _gl.Enable(EnableCap.DepthTest);
    }

    private unsafe void RenderNode(UiNode node)
    {
        if (node.Style.Display == DisplayMode.None)
            return;

        // Draw background if it has color (alpha > 0)
        if (node.Style.BackgroundColor.W > 0.001f)
        {
            _gl.BindVertexArray(_vao);
            _shader.SetInt("uHasTexture", 0);

            var model = Matrix4x4.CreateScale(node.ComputedSize.X, node.ComputedSize.Y, 1f) *
                        Matrix4x4.CreateTranslation(node.ComputedPosition.X, node.ComputedPosition.Y, 0f);

            _shader.SetMatrix4("uModel", model);
            _shader.SetVector4("uColor", node.Style.BackgroundColor);

            _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, null);
        }

        // Render text content
        if (!string.IsNullOrEmpty(node.TextContent) && _fontAtlas != null)
        {
            RenderText(node.TextContent, node.ComputedPosition, node.ComputedSize, node.Style);
        }

        // Render children
        for (int i = 0; i < node.Children.Count; i++)
            RenderNode(node.Children[i]);
    }

    private unsafe void RenderText(string text, Vector2 nodePos, Vector2 nodeSize, UiStyle style)
    {
        float fontSize = style.FontSize;
        float scale = fontSize / FontAtlas.GlyphHeight;
        float charW = FontAtlas.GlyphWidth * scale;
        float charH = FontAtlas.GlyphHeight * scale;

        float totalWidth = text.Length * charW;

        // Center text in node
        float startX = nodePos.X + (nodeSize.X - totalWidth) / 2f;
        float startY = nodePos.Y + (nodeSize.Y - charH) / 2f;

        // Clamp to node bounds
        startX = MathF.Max(startX, nodePos.X + style.PaddingLeft);
        startY = MathF.Max(startY, nodePos.Y + style.PaddingTop);

        _gl.BindVertexArray(_textVao);
        _shader.SetInt("uHasTexture", 1);
        _shader.SetVector4("uColor", style.TextColor);

        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _fontAtlas!.TextureId);
        _shader.SetInt("uTexture", 0);

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c < 32 || c > 126) c = '?';

            var (u, v, uSize, vSize) = _fontAtlas.GetCharUV(c);

            float x = startX + i * charW;
            float y = startY;

            // Check bounds
            if (x + charW > nodePos.X + nodeSize.X) break;

            float[] quadVerts =
            {
                x,          y,          0f,   u,         v,
                x + charW,  y,          0f,   u + uSize, v,
                x + charW,  y + charH,  0f,   u + uSize, v + vSize,
                x,          y + charH,  0f,   u,         v + vSize,
            };

            var model = Matrix4x4.Identity;
            _shader.SetMatrix4("uModel", model);

            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _textVbo);
            fixed (float* vp = quadVerts)
                _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (nuint)(quadVerts.Length * sizeof(float)), vp);

            _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, null);
        }

        _gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Dispose()
    {
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteBuffer(_ebo);
        _gl.DeleteVertexArray(_textVao);
        _gl.DeleteBuffer(_textVbo);
        _gl.DeleteBuffer(_textEbo);
        _fontAtlas?.Dispose();
    }
}
