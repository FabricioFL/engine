# Event Bus

The event bus provides decoupled pub/sub communication between systems using generic struct-based events. No boxing, no allocations for value types.

## Usage

Access via DI:

```csharp
var events = services.GetRequiredService<EventBus>();
```

## Defining Events

Events are plain structs:

```csharp
public struct PlayerDiedEvent
{
    public Entity Player;
    public Vector3 Position;
}

public struct SceneLoadedEvent
{
    public string SceneName;
}

public struct DamageEvent
{
    public Entity Source;
    public Entity Target;
    public float Amount;
}
```

## Subscribing

```csharp
events.Subscribe<PlayerDiedEvent>(OnPlayerDied);

void OnPlayerDied(PlayerDiedEvent evt)
{
    logger.Info("Game", $"Player died at {evt.Position}");
    ShowDeathScreen();
}
```

## Publishing

```csharp
events.Publish(new PlayerDiedEvent
{
    Player = playerEntity,
    Position = playerPosition
});
```

## Unsubscribing

```csharp
events.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
```

## Clearing

Remove all listeners for an event type:

```csharp
events.Clear<PlayerDiedEvent>();
```

## Guidelines

- Use structs for events — avoids GC pressure
- Subscribe in `Initialize` or `OnLoad`, unsubscribe in `Shutdown` or `OnUnload`
- Events are dispatched synchronously — the handler runs immediately when `Publish` is called
- Use events for cross-system communication (physics notifies game logic, UI reacts to game state changes) instead of direct references between systems
