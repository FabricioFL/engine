using Engine.Assets;
using Engine.ECS;
using Engine.ECS.Components;
using Engine.Rendering.Lighting;
using Engine.Rendering.PostProcess;
using Silk.NET.OpenGL;

namespace Engine.Rendering;

public sealed class RenderPipeline : IDisposable
{
    private readonly GL _gl;
    private readonly Camera _camera;
    private readonly AssetManager _assets;
    private LightManager? _lightManager;

    private readonly List<Mesh> _meshes = new();
    private readonly List<Material> _materials = new();

    // Post-processing
    private FrameBuffer? _sceneFbo;
    private FrameBuffer? _postFboA;
    private FrameBuffer? _postFboB;
    private BloomPass? _bloomPass;
    private FogPass? _fogPass;
    private Mesh? _screenQuad;
    private Shader? _blitShader;
    private uint _width;
    private uint _height;

    public Camera Camera => _camera;
    public LightManager? LightManager => _lightManager;
    public BloomPass? Bloom => _bloomPass;
    public FogPass? Fog => _fogPass;

    /// <summary>Called after scene meshes are rendered but before post-processing.</summary>
    public Action? OnSceneRendered { get; set; }

    public RenderPipeline(GL gl, Camera camera, AssetManager assets)
    {
        _gl = gl;
        _camera = camera;
        _assets = assets;
    }

    public void Initialize()
    {
        _lightManager = new LightManager(_gl);
    }

    public void InitializePostProcessing(uint width, uint height)
    {
        _width = width;
        _height = height;

        _screenQuad = MeshFactory.CreateScreenQuad(_gl);

        _sceneFbo = new FrameBuffer(_gl, width, height, true);
        _postFboA = new FrameBuffer(_gl, width, height, false);
        _postFboB = new FrameBuffer(_gl, width, height, false);

        var extractShader = _assets.LoadShader("bloom_extract",
            "assets/shaders/postprocess.vert", "assets/shaders/bloom_extract.frag");
        var blurShader = _assets.LoadShader("bloom_blur",
            "assets/shaders/postprocess.vert", "assets/shaders/bloom_blur.frag");
        var compositeShader = _assets.LoadShader("bloom_composite",
            "assets/shaders/postprocess.vert", "assets/shaders/bloom_composite.frag");
        var fogShader = _assets.LoadShader("fog",
            "assets/shaders/postprocess.vert", "assets/shaders/fog.frag");

        _blitShader = _assets.LoadShader("blit",
            "assets/shaders/postprocess.vert", "assets/shaders/blit.frag");

        _bloomPass = new BloomPass(_gl, width, height, _screenQuad, extractShader, blurShader, compositeShader);
        _fogPass = new FogPass(_gl, _screenQuad, fogShader);
    }

    public void ResizePostProcessing(uint width, uint height)
    {
        _width = width;
        _height = height;

        _sceneFbo?.Dispose();
        _postFboA?.Dispose();
        _postFboB?.Dispose();
        _bloomPass?.Dispose();

        _sceneFbo = new FrameBuffer(_gl, width, height, true);
        _postFboA = new FrameBuffer(_gl, width, height, false);
        _postFboB = new FrameBuffer(_gl, width, height, false);

        if (_screenQuad != null)
        {
            var extractShader = _assets.LoadShader("bloom_extract",
                "assets/shaders/postprocess.vert", "assets/shaders/bloom_extract.frag");
            var blurShader = _assets.LoadShader("bloom_blur",
                "assets/shaders/postprocess.vert", "assets/shaders/bloom_blur.frag");
            var compositeShader = _assets.LoadShader("bloom_composite",
                "assets/shaders/postprocess.vert", "assets/shaders/bloom_composite.frag");

            _bloomPass = new BloomPass(_gl, width, height, _screenQuad, extractShader, blurShader, compositeShader);
        }
    }

    public int RegisterMesh(Mesh mesh)
    {
        _meshes.Add(mesh);
        return _meshes.Count - 1;
    }

    public int RegisterMaterial(Material material)
    {
        _materials.Add(material);
        return _materials.Count - 1;
    }

    public Mesh? GetMesh(int handle) => handle >= 0 && handle < _meshes.Count ? _meshes[handle] : null;
    public Material? GetMaterial(int handle) => handle >= 0 && handle < _materials.Count ? _materials[handle] : null;

    public void Render(EntityManager entities)
    {
        _camera.UpdateMatrices();
        _lightManager?.UpdateLights(entities);

        bool hasPostProcess = _sceneFbo != null && _screenQuad != null;

        if (hasPostProcess)
        {
            _sceneFbo!.Bind();
            _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        _gl.Enable(EnableCap.DepthTest);
        RenderMeshes(entities);
        RenderSprites(entities);

        // Custom scene rendering (rain, particles, etc.)
        OnSceneRendered?.Invoke();

        if (hasPostProcess)
        {
            _gl.Disable(EnableCap.DepthTest);

            FrameBuffer current = _sceneFbo!;

            // Apply fog
            if (_fogPass != null && _fogPass.Enabled)
            {
                _fogPass.Apply(_sceneFbo!, _postFboA!, _camera);
                current = _postFboA!;
            }

            // Apply bloom
            if (_bloomPass != null)
            {
                var output = current == _postFboA ? _postFboB! : _postFboA!;
                _bloomPass.Apply(current, output);
                current = output;
            }

            // Blit to screen
            FrameBuffer.Unbind(_gl);
            _gl.Viewport(0, 0, _width, _height);
            _gl.Clear(ClearBufferMask.ColorBufferBit);

            _blitShader!.Use();
            current.BindColorTexture(TextureUnit.Texture0);
            _blitShader.SetInt("uScene", 0);
            _screenQuad!.Draw();

            _gl.Enable(EnableCap.DepthTest);
        }
    }

    private void RenderMeshes(EntityManager entities)
    {
        var renderPool = entities.GetPool<RenderComponent>();
        var transformPool = entities.GetPool<TransformComponent>();
        var renderSpan = renderPool.AsSpan();

        for (int i = 0; i < renderSpan.Length; i++)
        {
            ref var render = ref renderSpan[i];
            if (!render.Visible) continue;

            int entityId = renderPool.GetEntityAtIndex(i);
            if (!transformPool.Has(entityId)) continue;

            ref var transform = ref transformPool.Get(entityId);
            transform.UpdateMatrix();

            var mesh = GetMesh(render.MeshHandle);
            var material = GetMaterial(render.MaterialHandle);
            if (mesh == null || material == null) continue;

            bool transparent = material.Color.W < 0.99f;
            if (transparent)
            {
                _gl.Enable(EnableCap.Blend);
                _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            }

            material.Apply();
            material.Shader.SetMatrix4("uModel", transform.LocalToWorld);
            material.Shader.SetMatrix4("uView", _camera.ViewMatrix);
            material.Shader.SetMatrix4("uProjection", _camera.ProjectionMatrix);
            material.Shader.SetVector3("uViewPos", _camera.Position);

            if (_lightManager != null)
                _lightManager.BindToShader(material.Shader);

            mesh.Draw();

            if (transparent)
                _gl.Disable(EnableCap.Blend);
        }
    }

    private void RenderSprites(EntityManager entities)
    {
        var spritePool = entities.GetPool<SpriteComponent>();
        var transformPool = entities.GetPool<TransformComponent>();
        var spriteSpan = spritePool.AsSpan();

        for (int i = 0; i < spriteSpan.Length; i++)
        {
            ref var sprite = ref spriteSpan[i];
            int entityId = spritePool.GetEntityAtIndex(i);
            if (!transformPool.Has(entityId)) continue;

            ref var transform = ref transformPool.Get(entityId);
            transform.UpdateMatrix();
        }
    }

    public void Dispose()
    {
        _lightManager?.Dispose();
        _sceneFbo?.Dispose();
        _postFboA?.Dispose();
        _postFboB?.Dispose();
        _bloomPass?.Dispose();
        _screenQuad?.Dispose();
        foreach (var mesh in _meshes) mesh.Dispose();
        foreach (var mat in _materials) mat.Shader.Dispose();
    }
}
