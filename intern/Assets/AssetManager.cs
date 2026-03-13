using Silk.NET.OpenGL;
using Engine.Rendering;
using Shader = Engine.Rendering.Shader;
using Texture2D = Engine.Rendering.Texture2D;
using Mesh = Engine.Rendering.Mesh;

namespace Engine.Assets;

public sealed class AssetManager : IDisposable
{
    private readonly GL _gl;
    private readonly ShaderLibrary _shaders;
    private readonly Dictionary<string, Texture2D> _textures = new();
    private readonly Dictionary<string, Mesh> _meshes = new();
    private readonly string _basePath;

    public ShaderLibrary Shaders => _shaders;

    public AssetManager(GL gl)
    {
        _gl = gl;
        _shaders = new ShaderLibrary(gl);

        // Try both base directory (published) and working directory (development)
        _basePath = File.Exists(Path.Combine(AppContext.BaseDirectory, "config", "assets.json"))
            ? AppContext.BaseDirectory
            : Directory.GetCurrentDirectory();
    }

    public string ResolvePath(string relativePath)
    {
        return Path.Combine(_basePath, relativePath);
    }

    public Shader LoadShader(string name, string vertexRelPath, string fragmentRelPath)
    {
        return _shaders.Load(name, ResolvePath(vertexRelPath), ResolvePath(fragmentRelPath));
    }

    public Texture2D LoadTexture(string name, string relativePath)
    {
        if (_textures.TryGetValue(name, out var existing))
            return existing;

        var tex = new Texture2D(_gl, ResolvePath(relativePath));
        _textures[name] = tex;
        return tex;
    }

    public Texture2D GetTexture(string name) => _textures[name];

    public void RegisterMesh(string name, Mesh mesh)
    {
        _meshes[name] = mesh;
    }

    public Mesh GetMesh(string name) => _meshes[name];

    public void Dispose()
    {
        _shaders.Dispose();
        foreach (var tex in _textures.Values) tex.Dispose();
        foreach (var mesh in _meshes.Values) mesh.Dispose();
    }
}
