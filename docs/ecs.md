# Entity Component System

The ECS is the foundation of the engine. Entities are lightweight IDs, components are data-only structs stored in contiguous arrays, and systems contain the logic.

## Entities

An entity is just an ID with a generation counter:

```csharp
Entity player = entities.CreateEntity();
entities.DestroyEntity(player);
bool alive = entities.IsAlive(player);
```

## Components

Components are structs that implement `IComponent`. They hold data only — no logic. The engine provides built-in components, and you can create your own.

### Adding and Accessing Components

```csharp
// Add
entities.AddComponent(entity, TransformComponent.Default with
{
    Position = new Vector3(0, 5, 0)
});

// Get (returns a ref for direct mutation)
ref var transform = ref entities.GetComponent<TransformComponent>(entity);
transform.Position.Y += 1;

// Check
bool has = entities.HasComponent<TransformComponent>(entity);

// Remove
entities.RemoveComponent<TransformComponent>(entity);
```

### Built-in Components

#### TransformComponent
Position, rotation, and scale in world space.

```csharp
entities.AddComponent(entity, TransformComponent.Default with
{
    Position = new Vector3(0, 1, 0),
    Rotation = Quaternion.Identity,
    Scale = new Vector3(1, 2, 1)
});
```

- `Matrix4x4 LocalToWorld` — computed world matrix
- `bool IsDirty` — set true when modified, cleared after matrix update
- `void UpdateMatrix()` — recompute the world matrix

#### RenderComponent
Links an entity to a mesh and material for rendering.

```csharp
int meshH = renderer.RegisterMesh(mesh);
int matH = renderer.RegisterMaterial(material);

entities.AddComponent(entity, RenderComponent.Default with
{
    MeshHandle = meshH,
    MaterialHandle = matH,
    Visible = true,
    CastShadow = true
});
```

#### RigidbodyComponent
Links an entity to a physics body.

```csharp
// Dynamic body with mass
entities.AddComponent(entity, RigidbodyComponent.Dynamic(mass: 1.0f) with
{
    BodyHandle = bodyHandle.Value,
    UseGravity = true
});

// Kinematic body (moved by code, not forces)
entities.AddComponent(entity, RigidbodyComponent.Kinematic());
```

#### HealthComponent
Tracks entity health with damage cooldown.

```csharp
entities.AddComponent(entity, HealthComponent.Create(maxHealth: 100f, cooldown: 0.5f));

ref var health = ref entities.GetComponent<HealthComponent>(entity);
health.TakeDamage(25f);
health.UpdateCooldown(time.DeltaTime);

if (!health.IsAlive) { /* handle death */ }
```

- `float HealthPercent` — 0 to 1
- `bool CanTakeDamage` — respects cooldown timer

#### LightComponent
Attaches a light source to an entity.

```csharp
// Directional light (sun)
entities.AddComponent(entity, LightComponent.Directional(
    direction: new Vector3(-0.5f, -1f, -0.3f),
    color: new Vector3(0.9f, 0.85f, 0.7f),
    intensity: 0.8f
));

// Point light (lamp, fire)
entities.AddComponent(entity, LightComponent.Point(
    color: new Vector3(1f, 0.8f, 0.5f),
    intensity: 2.0f,
    range: 15f
));
```

The engine supports up to 16 lights, uploaded to the GPU via UBO.

#### ColliderComponent
Defines a physics collision shape.

```csharp
entities.AddComponent(entity, ColliderComponent.Box(
    halfExtents: new Vector3(0.5f, 1f, 0.5f),
    isTrigger: false
));

entities.AddComponent(entity, ColliderComponent.Sphere(radius: 1f, isTrigger: true));
entities.AddComponent(entity, ColliderComponent.Capsule(radius: 0.3f, length: 1.5f, isTrigger: false));
```

#### HazardComponent
Marks an entity as a damage source.

```csharp
entities.AddComponent(entity, HazardComponent.Create(damage: 10f, range: 2f));
```

#### SpriteComponent
For 2D billboard sprites in 3D space.

```csharp
entities.AddComponent(entity, SpriteComponent.Default with
{
    TextureHandle = texHandle,
    SpriteSize = new Vector2(1f, 1f),
    Billboard = BillboardMode.Vertical, // Full, Vertical, or None
    FlipX = false
});
```

#### ScriptComponent
Attaches a script to an entity for custom per-entity behavior.

```csharp
entities.AddComponent(entity, new ScriptComponent { ScriptIndex = scriptIdx });
```

#### SkeletonComponent
Attaches a bone hierarchy for animation. See [Animation](animation.md).

### Custom Components

Define your own components as structs:

```csharp
public struct InventoryComponent : IComponent
{
    public int Slots;
    public int UsedSlots;
}

entities.AddComponent(entity, new InventoryComponent { Slots = 10 });
```

## Systems

Systems contain game logic that operates on entities with specific components. Extend `SystemBase`:

```csharp
public class GravitySystem : SystemBase
{
    public override int Priority => 20; // Lower = runs earlier

    public override void Update(EntityManager entities, in GameTime time)
    {
        var pool = entities.GetPool<RigidbodyComponent>();
        var transforms = entities.GetPool<TransformComponent>();

        for (int i = 0; i < pool.AsSpan().Length; i++)
        {
            ref var rb = ref pool.AsSpan()[i];
            int entityId = pool.GetEntityAtIndex(i);

            if (transforms.Has(entityId))
            {
                ref var t = ref transforms.Get(entityId);
                t.Position.Y -= 9.81f * time.DeltaTime;
                t.IsDirty = true;
            }
        }
    }
}
```

Register systems with the scheduler:

```csharp
var scheduler = services.GetRequiredService<SystemScheduler>();
scheduler.Register(new GravitySystem());
```

Systems run in priority order each frame. Disable a system at runtime:

```csharp
system.Enabled = false;
```

## ComponentPool

Direct access to all components of a type for bulk operations:

```csharp
var pool = entities.GetPool<TransformComponent>();
Span<TransformComponent> all = pool.AsSpan();

for (int i = 0; i < all.Length; i++)
{
    ref var t = ref all[i];
    int entityId = pool.GetEntityAtIndex(i);
    // process...
}
```

## Scripts

Scripts provide per-entity behavior without creating a full system. Implement `IScript`:

```csharp
public class PatrolScript : IScript
{
    public void OnAttach(Entity entity, IServiceProvider services) { }
    public void OnUpdate(Entity entity, in GameTime time) { }
    public void OnDetach(Entity entity) { }
}
```
