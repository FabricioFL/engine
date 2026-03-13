using Silk.NET.OpenGL;

namespace Engine.Rendering;

public sealed class ShaderLibrary : IDisposable
{
    private readonly GL _gl;
    private readonly Dictionary<string, Shader> _shaders = new();

    public ShaderLibrary(GL gl)
    {
        _gl = gl;
    }

    public Shader Load(string name, string vertexPath, string fragmentPath)
    {
        if (_shaders.TryGetValue(name, out var existing))
            return existing;

        string vertSource = File.ReadAllText(vertexPath);
        string fragSource = File.ReadAllText(fragmentPath);

        var shader = new Shader(_gl, vertSource, fragSource);
        _shaders[name] = shader;
        return shader;
    }

    public Shader Get(string name) => _shaders[name];

    public bool TryGet(string name, out Shader shader) => _shaders.TryGetValue(name, out shader!);

    public void Dispose()
    {
        foreach (var shader in _shaders.Values)
            shader.Dispose();
        _shaders.Clear();
    }
}
