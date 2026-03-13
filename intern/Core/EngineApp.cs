using Microsoft.Extensions.DependencyInjection;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Engine.Logging;
using Engine.Config;
using Engine.ECS;
using Engine.Rendering;
using Engine.Input;
using Engine.Scene;
using Engine.Events;
using Engine.Memory;
using Engine.Assets;
using Engine.Physics;
using Engine.UI.Runtime;
using Engine.UI.Rendering;

namespace Engine.Core;

public sealed class EngineApp
{
    private IWindow _window = null!;
    private GL _gl = null!;
    private IServiceProvider _services = null!;
    private IGame _game = null!;
    private ILogger _logger = null!;

    private double _totalTime;
    private long _frameCount;

    private EntityManager _entityManager = null!;
    private SystemScheduler _systemScheduler = null!;
    private InputManager _inputManager = null!;
    private SceneManager _sceneManager = null!;
    private RenderPipeline _renderPipeline = null!;
    private ArenaAllocator _frameAllocator = null!;
    private UiManager _uiManager = null!;
    private UiRenderer _uiRenderer = null!;

    public void Run<TGame>(string title = "Engine", int width = 800, int height = 600) where TGame : class, IGame, new()
    {
        var options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(width, height),
            Title = title,
            API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(3, 3))
        };

        _window = Window.Create(options);
        _window.Load += () => OnLoad<TGame>();
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Closing += OnClosing;
        _window.Run();
    }

    private void OnLoad<TGame>() where TGame : class, IGame, new()
    {
        _gl = _window.CreateOpenGL();
        _gl.ClearColor(0f, 0f, 0f, 1f);
        _gl.Enable(EnableCap.DepthTest);
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        _services = serviceCollection.BuildServiceProvider();

        _logger = _services.GetRequiredService<ILogger>();
        _entityManager = _services.GetRequiredService<EntityManager>();
        _systemScheduler = _services.GetRequiredService<SystemScheduler>();
        _inputManager = _services.GetRequiredService<InputManager>();
        _sceneManager = _services.GetRequiredService<SceneManager>();
        _renderPipeline = _services.GetRequiredService<RenderPipeline>();
        _frameAllocator = _services.GetRequiredService<ArenaAllocator>();

        _inputManager.Initialize(_window.CreateInput());
        _renderPipeline.Initialize();

        // Post-processing
        _renderPipeline.InitializePostProcessing((uint)_window.Size.X, (uint)_window.Size.Y);

        // Physics
        var physicsSystem = _services.GetRequiredService<PhysicsSystem>();
        _systemScheduler.Register(physicsSystem);

        // UI
        _uiManager = _services.GetRequiredService<UiManager>();
        var assets = _services.GetRequiredService<AssetManager>();
        var uiShader = assets.LoadShader("ui", "assets/shaders/ui.vert", "assets/shaders/ui.frag");
        _uiRenderer = new UiRenderer(_gl, uiShader);
        _uiRenderer.UpdateProjection(_window.Size.X, _window.Size.Y);

        string basePath = File.Exists(Path.Combine(AppContext.BaseDirectory, "config", "ui.json"))
            ? AppContext.BaseDirectory
            : Directory.GetCurrentDirectory();
        _uiManager.Initialize(_window.Size.X, _window.Size.Y, basePath);

        // Set initial camera aspect ratio
        _renderPipeline.Camera.AspectRatio = (float)_window.Size.X / _window.Size.Y;

        void HandleResize(int w, int h)
        {
            _gl.Viewport(0, 0, (uint)w, (uint)h);
            _renderPipeline.Camera.AspectRatio = (float)w / h;
            _renderPipeline.ResizePostProcessing((uint)w, (uint)h);
            _uiRenderer.UpdateProjection(w, h);
            _uiManager.UpdateScreenSize(w, h);
        }

        _window.Resize += size => HandleResize(size.X, size.Y);
        _window.FramebufferResize += size => HandleResize(size.X, size.Y);

        _game = new TGame();
        _game.Initialize(_services);
        _game.LoadContent();

        _sceneManager.LoadDefaultScene();

        _logger.Info("Core", "Engine initialized successfully");
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(_gl);
        services.AddSingleton(_window);

        // Memory
        services.AddSingleton(new ArenaAllocator(1024 * 1024));

        // Events
        services.AddSingleton<EventBus>();

        // Logging
        var logConfig = ConfigLoader.Load<LogConfig>("config/log.json");
        services.AddSingleton(logConfig);
        services.AddSingleton<ILogger, Logger>();

        // Config
        var uiConfig = ConfigLoader.Load<UiConfig>("config/ui.json");
        var sceneConfig = ConfigLoader.Load<SceneConfig>("config/scenes.json");
        var skillsConfig = ConfigLoader.Load<SkillsConfig>("config/skills.json");
        var assetsConfig = ConfigLoader.Load<AssetsConfig>("config/assets.json");
        services.AddSingleton(uiConfig);
        services.AddSingleton(sceneConfig);
        services.AddSingleton(skillsConfig);
        services.AddSingleton(assetsConfig);

        // ECS
        services.AddSingleton<EntityManager>();
        services.AddSingleton<SystemScheduler>();

        // Input
        services.AddSingleton<InputManager>();

        // Rendering
        services.AddSingleton<RenderPipeline>();
        services.AddSingleton<Camera>();

        // Assets
        services.AddSingleton<AssetManager>();

        // Physics
        services.AddSingleton<PhysicsWorld>();
        services.AddSingleton<PhysicsSystem>();

        // UI
        services.AddSingleton<UiManager>();

        // Scene
        services.AddSingleton<SceneManager>();
    }

    private void OnUpdate(double deltaTime)
    {
        _frameAllocator.Reset();

        float dt = (float)deltaTime;
        _totalTime += deltaTime;
        _frameCount++;

        var gameTime = new GameTime(dt, _totalTime, _frameCount);

        _inputManager.Update();

        _systemScheduler.RunAll(_entityManager, in gameTime);
        _game.Update(in gameTime);
        _uiManager.Update(in gameTime);
        _sceneManager.ProcessPendingTransitions();
    }

    private unsafe void OnRender(double deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _renderPipeline.Render(_entityManager);
        _uiRenderer.Render(_uiManager);
    }

    private void OnClosing()
    {
        _game.Shutdown();
        _logger.Info("Core", "Engine shutting down");
        _gl.Dispose();
    }
}
