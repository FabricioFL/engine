# Physics

The engine uses BepuPhysics2 for 3D physics simulation — a high-performance, zero-allocation physics library with SIMD support.

## PhysicsWorld

Access via DI:

```csharp
var physics = services.GetRequiredService<PhysicsWorld>();
```

The engine steps the physics simulation automatically each frame and syncs body poses back to ECS `TransformComponent`.

## Creating Collision Shapes

```csharp
TypedIndex boxShape = physics.CreateBox(width: 1f, height: 1f, depth: 1f);
TypedIndex sphereShape = physics.CreateSphere(radius: 0.5f);
TypedIndex capsuleShape = physics.CreateCapsule(radius: 0.3f, length: 1.5f);
```

## Dynamic Bodies

Dynamic bodies are affected by forces, gravity, and collisions:

```csharp
var shape = physics.CreateBox(1f, 2f, 1f);
BodyHandle body = physics.AddDynamicBody(
    position: new Vector3(0, 5, 0),
    rotation: Quaternion.Identity,
    mass: 1.0f,
    shape: shape
);
```

Link the body to an entity:

```csharp
entities.AddComponent(entity, RigidbodyComponent.Dynamic(mass: 1.0f) with
{
    BodyHandle = body.Value,
    UseGravity = true
});
```

## Static Bodies

Static bodies don't move but participate in collisions (floors, walls):

```csharp
var groundShape = physics.CreateBox(50f, 0.5f, 50f);
StaticHandle ground = physics.AddStaticBody(
    position: new Vector3(0, -0.25f, 0),
    rotation: Quaternion.Identity,
    shape: groundShape
);
```

Static bodies don't need a `RigidbodyComponent` — they exist only in the physics world.

## Kinematic Bodies

Kinematic bodies are moved by code (not forces) but still collide with dynamic bodies:

```csharp
entities.AddComponent(entity, RigidbodyComponent.Kinematic() with
{
    BodyHandle = body.Value
});
```

## Applying Forces

```csharp
// Set velocity directly
physics.SetBodyVelocity(bodyHandle, new Vector3(0, 5, 0));

// Apply an impulse
physics.ApplyImpulse(bodyHandle, new Vector3(100, 0, 0));
```

Both methods automatically wake sleeping bodies.

## Reading Body State

```csharp
(Vector3 position, Quaternion rotation) = physics.GetBodyPose(bodyHandle);
```

The `PhysicsSystem` automatically copies body poses to `TransformComponent` each frame — you don't need to do this manually.

## Removing Bodies

```csharp
physics.RemoveBody(bodyHandle);     // Dynamic/kinematic
physics.RemoveStatic(staticHandle); // Static
```

## Gravity

```csharp
physics.Gravity = new Vector3(0, -9.81f, 0); // Default
```

## Invisible Walls

For invisible boundaries (no visual, physics only):

```csharp
var wallShape = physics.CreateBox(1f, 10f, 50f);
physics.AddStaticBody(new Vector3(25, 5, 0), Quaternion.Identity, wallShape);
// Don't add a RenderComponent — no entity needed if no game logic references it
```

## Direct Simulation Access

For advanced Bepu usage:

```csharp
Simulation sim = physics.Simulation;
```
