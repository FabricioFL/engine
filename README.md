# Engine

A code-first 3D game engine built with C# and OpenGL 3.3, designed for lightweight lowpoly/pixel-art games with 2D character sprites in 3D worlds.

## Disclaimer

This engine is built and maintained by me for my own use and my studio's projects. I update it based on my own demands and priorities.

**This is not an open-source project by definition.** You are free to clone or fork the repository, but I will not review or accept pull requests.

**I do not recommend using this engine in any commercial project.** It is a personal tool with no stability guarantees, no support, and no commitment to backwards compatibility.

## Features

- **Entity Component System** — struct-based components stored in contiguous arrays, systems with priority ordering, scripts and skills interfaces
- **OpenGL 3.3 Rendering** — mesh and sprite rendering, material system, shader library, texture atlas support
- **Lighting** — directional and point lights (max 16) via UBO, ambient color control
- **Post-Processing** — bloom (bright extract + gaussian blur) and depth-based fog via framebuffer passes
- **Weather Effects** — particle-based rain renderer
- **Physics** — BepuPhysics2 integration with dynamic/static/kinematic bodies, box/sphere/capsule colliders, impulse and velocity control
- **Skeleton Animation** — bone hierarchies, animation clips with keyframes, procedural animation API, skinning matrices
- **Input** — keyboard, mouse, and gamepad via Silk.NET, cursor locking, per-frame press/release detection
- **UI System** — HTML/CSS-like markup parsed at runtime, flexbox-lite layout engine, controller pattern with click/text/visibility bindings
- **Scene Management** — named scenes with entity and system ownership, queued transitions, default scene loading
- **Audio** — Silk.NET OpenAL integration
- **Networking** — TCP server/client, UDP server/client, HTTP helper, zero-alloc packet serialization
- **Event Bus** — generic struct-based pub/sub, no boxing
- **Logging** — leveled logging with console and file sinks
- **Config** — JSON config files with System.Text.Json source generators (no reflection)
- **Memory** — arena allocator, object pooling, native buffers — zero GC in hot paths
- **Dependency Injection** — Microsoft.Extensions.DependencyInjection for all engine services

## Architecture

The engine follows a clone-per-project model:

```
engine/
  Program.cs              # Thin bootstrap → EngineApp.Run()
  intern/                 # Engine internals (don't modify)
  src/                    # Your game code (implement IGame)
  ui/
    gui/                  # HTML/CSS templates
    control/              # C# UI controllers
  config/                 # JSON configuration files
  assets/
    shaders/ textures/ models/ audio/ fonts/ rigs/
  logs/                   # Runtime logs (gitignored)
```

## Getting Started

1. Clone the repository
2. Implement `IGame` in `src/Game.cs`
3. Run with `dotnet run`

```csharp
public class Game : IGame
{
    public void Initialize(IServiceProvider services) { }
    public void LoadContent() { }
    public void Update(in GameTime time) { }
    public void Shutdown() { }
}
```

See `src/Game.cs` for a complete example with player movement, physics, UI, lighting, and weather.

## Dependencies

| Package | Version | Purpose |
|---|---|---|
| Silk.NET | 2.22.0 | Window, OpenGL, Input, Audio |
| BepuPhysics | 2.4.0 | 3D physics simulation |
| StbImageSharp | 2.27.14 | Texture loading |
| System.Text.Json | 8.0.5 | Config parsing |
| Microsoft.Extensions.DependencyInjection | 8.0.0 | Service wiring |

**Runtime:** .NET 9.0, OpenGL 3.3+

## Documentation

Full documentation is available in the [docs/](docs/) folder:

- [Getting Started](docs/getting-started.md) — project setup and first game
- [Core](docs/core.md) — EngineApp, IGame, GameTime, dependency injection
- [ECS](docs/ecs.md) — entities, components, systems, scripts
- [Rendering](docs/rendering.md) — meshes, materials, shaders, camera, lighting, post-processing
- [Physics](docs/physics.md) — rigid bodies, colliders, forces, collision
- [Input](docs/input.md) — keyboard, mouse, gamepad
- [UI](docs/ui.md) — HTML/CSS templates, controllers, layout
- [Animation](docs/animation.md) — skeletons, clips, procedural animation
- [Scene Management](docs/scenes.md) — scenes, transitions, lifecycle
- [Audio](docs/audio.md) — audio engine, clips, sources
- [Networking](docs/networking.md) — TCP, UDP, HTTP, packets
- [Events](docs/events.md) — event bus pub/sub
- [Config](docs/config.md) — JSON configuration files
- [Memory](docs/memory.md) — arena allocator, object pools, native buffers
- [Logging](docs/logging.md) — log levels, sinks, file rotation
