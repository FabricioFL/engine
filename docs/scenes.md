# Scene Management

Scenes organize entities and systems into logical groups. The scene manager handles transitions between them.

## SceneManager

Access via DI:

```csharp
var scenes = services.GetRequiredService<SceneManager>();
```

## Defining a Scene

Extend the `Scene` class:

```csharp
public class ForestScene : Scene
{
    public ForestScene() : base("forest") { }

    public override void OnLoad(EntityManager entities, IServiceProvider services)
    {
        // Create entities, set up environment
        var ground = entities.CreateEntity();
        AddEntity(ground);
        // ...
    }

    public override void OnUnload(EntityManager entities)
    {
        // Cleanup — entities tracked via AddEntity are removed
    }
}
```

## Registering Scenes

```csharp
scenes.Register("forest", new ForestScene());
scenes.Register("dungeon", new DungeonScene());
```

## Loading Scenes

```csharp
scenes.LoadScene("forest");
```

Scene transitions are queued and processed once per frame by the engine. The active scene is unloaded before the new one loads.

```csharp
Scene? current = scenes.ActiveScene;
```

## Scene Entities and Systems

Track entities and systems within a scene:

```csharp
public override void OnLoad(EntityManager entities, IServiceProvider services)
{
    var entity = entities.CreateEntity();
    AddEntity(entity);       // Track for cleanup

    var system = new WeatherSystem();
    AddSystem(system);       // Track for cleanup
}
```

Access tracked entities and systems:

```csharp
IReadOnlyList<Entity> sceneEntities = scene.Entities;
IReadOnlyList<SystemBase> sceneSystems = scene.Systems;
```

## Default Scene

Load the default scene (first registered or configured):

```csharp
scenes.LoadDefaultScene();
```

## Manual Scene Management

You can also manage scenes manually without the `Scene` class — create and destroy entities directly in your `IGame` methods, using the scene manager only for transitions:

```csharp
// In Game.cs
void StartGame()
{
    ClearMenuEntities();
    BuildGameEntities();
}

void ReturnToMenu()
{
    ClearGameEntities();
    BuildMenuEntities();
}
```

This approach gives you full control over entity lifecycle.
