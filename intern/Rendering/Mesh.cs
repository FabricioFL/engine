using Silk.NET.OpenGL;

namespace Engine.Rendering;

public sealed class Mesh : IDisposable
{
    private readonly GL _gl;
    private readonly uint _vao;
    private readonly uint _vbo;
    private readonly uint _ebo;
    private readonly uint _indexCount;

    public uint VAO => _vao;
    public uint IndexCount => _indexCount;

    public unsafe Mesh(GL gl, float[] vertices, uint[] indices, VertexLayout layout)
    {
        _gl = gl;
        _indexCount = (uint)indices.Length;

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (float* v = vertices)
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);

        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        fixed (uint* i = indices)
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);

        layout.Apply(_gl);

        _gl.BindVertexArray(0);
    }

    public void Bind() => _gl.BindVertexArray(_vao);

    public unsafe void Draw()
    {
        _gl.BindVertexArray(_vao);
        _gl.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, null);
    }

    public void Dispose()
    {
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteBuffer(_ebo);
    }
}

public readonly struct VertexLayout
{
    private readonly VertexAttribute[] _attributes;
    private readonly uint _stride;

    public VertexLayout(params VertexAttribute[] attributes)
    {
        _attributes = attributes;
        _stride = 0;
        for (int i = 0; i < attributes.Length; i++)
            _stride += attributes[i].SizeInBytes;
    }

    public unsafe void Apply(GL gl)
    {
        uint offset = 0;
        for (uint i = 0; i < _attributes.Length; i++)
        {
            gl.EnableVertexAttribArray(i);
            gl.VertexAttribPointer(i, (int)_attributes[i].ComponentCount,
                _attributes[i].Type, _attributes[i].Normalized, _stride, (void*)offset);
            offset += _attributes[i].SizeInBytes;
        }
    }

    // Standard layouts
    public static VertexLayout PositionOnly => new(
        new VertexAttribute(3, VertexAttribPointerType.Float) // position
    );

    public static VertexLayout PositionTexture => new(
        new VertexAttribute(3, VertexAttribPointerType.Float), // position
        new VertexAttribute(2, VertexAttribPointerType.Float)  // texcoord
    );

    public static VertexLayout PositionNormalTexture => new(
        new VertexAttribute(3, VertexAttribPointerType.Float), // position
        new VertexAttribute(3, VertexAttribPointerType.Float), // normal
        new VertexAttribute(2, VertexAttribPointerType.Float)  // texcoord
    );

    public static VertexLayout Full => new(
        new VertexAttribute(3, VertexAttribPointerType.Float), // position
        new VertexAttribute(3, VertexAttribPointerType.Float), // normal
        new VertexAttribute(2, VertexAttribPointerType.Float), // texcoord
        new VertexAttribute(4, VertexAttribPointerType.Float)  // color
    );
}

public readonly struct VertexAttribute
{
    public readonly uint ComponentCount;
    public readonly VertexAttribPointerType Type;
    public readonly bool Normalized;
    public readonly uint SizeInBytes;

    public VertexAttribute(uint componentCount, VertexAttribPointerType type, bool normalized = false)
    {
        ComponentCount = componentCount;
        Type = type;
        Normalized = normalized;
        SizeInBytes = componentCount * type switch
        {
            VertexAttribPointerType.Float => 4u,
            VertexAttribPointerType.Int => 4u,
            VertexAttribPointerType.UnsignedByte => 1u,
            _ => 4u
        };
    }
}
