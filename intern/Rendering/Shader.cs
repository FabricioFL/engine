using System.Numerics;
using Silk.NET.OpenGL;

namespace Engine.Rendering;

public sealed class Shader : IDisposable
{
    private readonly GL _gl;
    private readonly uint _handle;
    private readonly Dictionary<string, int> _uniformLocations = new();

    public uint Handle => _handle;

    public Shader(GL gl, string vertexSource, string fragmentSource)
    {
        _gl = gl;

        uint vertex = CompileShader(ShaderType.VertexShader, vertexSource);
        uint fragment = CompileShader(ShaderType.FragmentShader, fragmentSource);

        _handle = _gl.CreateProgram();
        _gl.AttachShader(_handle, vertex);
        _gl.AttachShader(_handle, fragment);
        _gl.LinkProgram(_handle);

        _gl.GetProgram(_handle, ProgramPropertyARB.LinkStatus, out int status);
        if (status != (int)GLEnum.True)
            throw new Exception($"Shader link error: {_gl.GetProgramInfoLog(_handle)}");

        _gl.DetachShader(_handle, vertex);
        _gl.DetachShader(_handle, fragment);
        _gl.DeleteShader(vertex);
        _gl.DeleteShader(fragment);
    }

    public void Use() => _gl.UseProgram(_handle);

    public void SetInt(string name, int value)
    {
        _gl.Uniform1(GetLocation(name), value);
    }

    public void SetFloat(string name, float value)
    {
        _gl.Uniform1(GetLocation(name), value);
    }

    public void SetVector2(string name, Vector2 value)
    {
        _gl.Uniform2(GetLocation(name), value.X, value.Y);
    }

    public void SetVector3(string name, Vector3 value)
    {
        _gl.Uniform3(GetLocation(name), value.X, value.Y, value.Z);
    }

    public void SetVector4(string name, Vector4 value)
    {
        _gl.Uniform4(GetLocation(name), value.X, value.Y, value.Z, value.W);
    }

    public unsafe void SetMatrix4(string name, Matrix4x4 value)
    {
        _gl.UniformMatrix4(GetLocation(name), 1, false, (float*)&value);
    }

    private int GetLocation(string name)
    {
        if (_uniformLocations.TryGetValue(name, out int location))
            return location;

        location = _gl.GetUniformLocation(_handle, name);
        _uniformLocations[name] = location;
        return location;
    }

    private uint CompileShader(ShaderType type, string source)
    {
        uint shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);

        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
        if (status != (int)GLEnum.True)
            throw new Exception($"{type} compile error: {_gl.GetShaderInfoLog(shader)}");

        return shader;
    }

    public void Dispose()
    {
        _gl.DeleteProgram(_handle);
    }
}
