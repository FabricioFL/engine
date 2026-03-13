# Core

The core module provides the engine bootstrap, game interface, and time tracking.

## EngineApp

The static entry point that creates the window, initializes all systems, and runs the game loop.

```csharp
EngineApp.Run<Game>("Window Title", 1280, 720);
```

### What It Does

1. Creates the window and OpenGL context via Silk.NET
2. Registers all engine services in the DI container
3. Loads configuration files from `config/`
4. Initializes rendering, physics, input, UI, and audio
5. Calls `IGame.Initialize()` and `IGame.LoadContent()`
6. Enters the main loop

### Main Loop Order

Each frame executes in this order:

```
ArenaAllocator.Reset()
InputManager.Update()
PhysicsSystem.Step(dt)           # Physics simulation
PhysicsSystem.SyncTransforms()   # Copy physics poses → ECS transforms
AnimatorSystem.Evaluate(dt)      # Skeleton animation
SystemScheduler.RunAll(dt)       # All registered systems by priority
IGame.Update(gameTime)           # Your game logic
UiManager.Update(gameTime)       # UI controller updates
SceneManager.ProcessTransitions()

— Render —
Camera.UpdateMatrices()
RenderPipeline.Render()          # Meshes, sprites, lights
PostProcess (Bloom, Fog)
UiRenderer.Render()              # UI overlay
```

## IGame

The interface your game class must implement:

```csharp
public interface IGame
{
    void Initialize(IServiceProvider services);
    void LoadContent();
    void Update(in GameTime time);
    void Shutdown();
}
```

| Method | When | Purpose |
|---|---|---|
| Initialize | Once, after engine ready | Get services from DI |
| LoadContent | Once, after Initialize | Load assets, create entities |
| Update | Every frame | Game logic |
| Shutdown | On exit | Cleanup |

## GameTime

A readonly struct passed to `Update` each frame:

```csharp
public readonly struct GameTime
{
    public float DeltaTime;    // Seconds since last frame
    public double TotalTime;   // Total elapsed seconds
    public long FrameCount;    // Current frame number
}
```

Use `DeltaTime` for frame-rate-independent movement:

```csharp
position += velocity * time.DeltaTime;
```

## Dependency Injection

All engine services are registered as singletons:

| Service | Type |
|---|---|
| EntityManager | ECS entity management |
| SystemScheduler | ECS system execution |
| RenderPipeline | Rendering |
| Camera | Camera control |
| InputManager | Input handling |
| PhysicsWorld | Physics simulation |
| AssetManager | Asset loading |
| SceneManager | Scene transitions |
| UiManager | UI system |
| EventBus | Event pub/sub |
| ILogger | Logging |
| ArenaAllocator | Frame memory |
| Config objects | UiConfig, SceneConfig, etc. |

Access them via `IServiceProvider` in your `Initialize` method:

```csharp
var input = services.GetRequiredService<InputManager>();
```
