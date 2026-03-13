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
