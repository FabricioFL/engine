# Memory Management

The engine is designed for zero GC allocations in hot paths. Several tools help achieve this.

## ArenaAllocator

A bump allocator that resets each frame. Use for temporary per-frame data:

```csharp
var arena = services.GetRequiredService<ArenaAllocator>();

// Allocate temporary buffer (valid until next frame)
Span<Vector3> positions = arena.Allocate<Vector3>(count: 100);
```

The engine calls `arena.Reset()` at the start of each frame. All allocations from the previous frame are invalidated — do not hold references across frames.

Thread-safe. Uses native memory (not managed heap).

## ObjectPool

Reuse objects to avoid allocation/GC:

```csharp
var pool = new ObjectPool<Particle>(
    factory: () => new Particle(),
    initialSize: 100
);

Particle p = pool.Rent();
// use particle...
pool.Return(p);
```

Good for: particles, network packets, audio sources, temporary objects with complex initialization.

## NativeBuffer

Unmanaged memory buffer for GPU uploads and interop:

```csharp
var buffer = new NativeBuffer<float>(1024);
// Write data...
// Upload to GPU...
buffer.Dispose(); // Must dispose manually
```

## Guidelines

| Technique | When to Use |
|---|---|
| ArenaAllocator | Temporary per-frame data (culling lists, sort buffers) |
| ObjectPool | Reusable objects with lifecycle (particles, packets) |
| NativeBuffer | GPU upload buffers, native interop |
| stackalloc / Span | Small, short-lived arrays (< 1KB) |
| ArrayPool\<T\>.Shared | Medium temporary arrays |
| Struct components | ECS data (stored in contiguous ComponentPool arrays) |

## Rules for Hot Paths

- No `new` for reference types in update/render loops
- No LINQ (allocates iterators and delegates)
- No delegates or lambdas (allocates closure objects)
- No string concatenation (use `Span<char>` or pre-allocated buffers)
- No boxing (avoid casting structs to interfaces in hot code)
- Use `in` parameters for readonly struct passing
- Use `ref` returns from `EntityManager.GetComponent<T>()` to avoid copies
