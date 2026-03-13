# Getting Started

## Requirements

- .NET 9.0 SDK
- GPU with OpenGL 3.3+ support

## Project Setup

Clone the engine repository. The project is ready to run out of the box:

```bash
dotnet run
```

## Project Structure

```
engine/
  Program.cs              # Bootstrap — calls EngineApp.Run<Game>()
  intern/                 # Engine code — do not modify
  src/
    Game.cs               # Your game — implements IGame
  ui/
    gui/                  # HTML/CSS UI templates
    control/              # C# UI controllers
  config/                 # JSON config files
  assets/
    shaders/              # GLSL vertex/fragment shaders
    textures/             # Image files
    models/               # 3D models
    audio/                # Sound files
    fonts/                # Font files
    rigs/                 # Skeleton/rig definitions
  logs/                   # Runtime log output (gitignored)
```

## Your First Game

The engine entry point is `Program.cs`:

```csharp
EngineApp.Run<Game>("My Game", 800, 600);
```

This creates a window, initializes all engine systems, and starts the game loop. Your game class must implement `IGame`:

```csharp
using Engine.Core;

public class Game : IGame
{
    private EntityManager _entities;
    private RenderPipeline _renderer;

    public void Initialize(IServiceProvider services)
    {
        _entities = services.GetRequiredService<EntityManager>();
        _renderer = services.GetRequiredService<RenderPipeline>();
    }

    public void LoadContent()
    {
        // Load shaders, create meshes, materials, and entities
    }

    public void Update(in GameTime time)
    {
        // Game logic runs every frame
        // time.DeltaTime = seconds since last frame
        // time.TotalTime = total elapsed seconds
    }

    public void Shutdown()
    {
        // Cleanup
    }
}
```

## IGame Lifecycle

1. **Initialize** — called once after all engine systems are ready. Use `IServiceProvider` to get engine services (EntityManager, RenderPipeline, InputManager, PhysicsWorld, etc.)
2. **LoadContent** — called once after Initialize. Load assets, create entities, build your scene.
3. **Update** — called every frame. Handle input, run game logic, update state.
4. **Shutdown** — called when the game exits. Release any custom resources.

## Accessing Engine Services

All engine systems are registered in the DI container. Get them in `Initialize`:

```csharp
var entities = services.GetRequiredService<EntityManager>();
var input = services.GetRequiredService<InputManager>();
var physics = services.GetRequiredService<PhysicsWorld>();
var renderer = services.GetRequiredService<RenderPipeline>();
var assets = services.GetRequiredService<AssetManager>();
var scenes = services.GetRequiredService<SceneManager>();
var ui = services.GetRequiredService<UiManager>();
var events = services.GetRequiredService<EventBus>();
var logger = services.GetRequiredService<ILogger>();
```

## Creating an Entity

```csharp
// Create entity
var entity = _entities.CreateEntity();

// Add components
_entities.AddComponent(entity, TransformComponent.Default with
{
    Position = new Vector3(0, 1, 0),
    Scale = new Vector3(1, 1, 1)
});

// Add a mesh and material for rendering
int meshHandle = _renderer.RegisterMesh(MeshFactory.CreateCube(gl));
int matHandle = _renderer.RegisterMaterial(new Material(shader)
{
    Color = new Vector4(1, 0, 0, 1) // Red
});

_entities.AddComponent(entity, RenderComponent.Default with
{
    MeshHandle = meshHandle,
    MaterialHandle = matHandle,
    Visible = true
});
```

## Running the Game

```bash
dotnet run
```

The window opens, and your game loop starts. The engine handles the update/render cycle automatically. Your `Update` method is called each frame between physics stepping and UI updates.

## Build Profiles

The engine supports three build profiles: **Debug**, **Preview**, and **Production**. Each profile controls optimization, logging, terminal visibility, and packaging.

| | Debug | Preview | Production |
|---|---|---|---|
| Standalone executable | No | Yes | Yes |
| Self-contained (.NET bundled) | No | Yes | Yes |
| Optimized | No | Yes | Yes (+ trimmed) |
| Terminal window | Visible | Hidden | Hidden |
| Log output (file sink) | All levels | All levels | Disabled |
| Console log sink | Active | Disabled | Disabled |

### Debug

Default profile for development. Runs from source with full logging to both console and file. The terminal window stays open so you can see log output in real time.

```bash
# Run directly (default configuration)
dotnet run

# Or explicitly
dotnet run -c Debug
```

No standalone binary is produced — the game runs through the .NET SDK.

### Preview

Standalone build with logging enabled (file sink only). The terminal window is hidden. Use this to test what the final game will look and feel like while still capturing logs for diagnostics.

```bash
# Build
dotnet publish -c Preview -r <RID> -o build/preview

# Examples per platform
dotnet publish -c Preview -r win-x64 -o build/preview
dotnet publish -c Preview -r linux-x64 -o build/preview
dotnet publish -c Preview -r osx-x64 -o build/preview
```

Output is a single self-contained executable in `build/preview/`. Logs are written to `logs/` next to the executable at runtime. No terminal window is shown.

### Production

Final standalone build for distribution. All logging is disabled, the terminal is hidden, and the binary is trimmed to reduce file size.

```bash
# Build
dotnet publish -c Production -r <RID> -o build/production

# Examples per platform
dotnet publish -c Production -r win-x64 -o build/production
dotnet publish -c Production -r linux-x64 -o build/production
dotnet publish -c Production -r osx-x64 -o build/production
```

Output is a single self-contained executable in `build/production/`. No logs are written and no terminal window is shown.

### Runtime Identifiers (RID)

The `-r` flag specifies the target platform. Common values:

| RID | Platform |
|---|---|
| `win-x64` | Windows 64-bit |
| `linux-x64` | Linux 64-bit |
| `osx-x64` | macOS Intel |
| `osx-arm64` | macOS Apple Silicon |

### Assets

Published builds automatically include `assets/`, `config/`, and `ui/gui/` alongside the executable. These folders must stay next to the binary at runtime.

### Preprocessor Constants

Each profile defines constants you can use in game code with `#if` directives:

```csharp
#if DEBUG
    logger.Info("Game", "Debug mode active");
#endif

#if ENABLE_LOGS
    logger.Info("Game", $"Player health: {health}");
#endif

#if PRODUCTION
    // Production-only code path
#endif
```

| Constant | Debug | Preview | Production |
|---|---|---|---|
| `DEBUG` | Yes | — | — |
| `ENABLE_LOGS` | Yes | Yes | — |
| `PREVIEW` | — | Yes | — |
| `PRODUCTION` | — | — | Yes |
