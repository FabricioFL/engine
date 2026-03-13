using System.Numerics;
using BepuPhysics;
using Engine.Core;
using Engine.ECS;
using Engine.ECS.Components;

namespace Engine.Physics;

public sealed class PhysicsSystem : SystemBase
{
    private readonly PhysicsWorld _physicsWorld;

    public override int Priority => 10; // Early in update

    public PhysicsSystem(PhysicsWorld physicsWorld)
    {
        _physicsWorld = physicsWorld;
    }

    public override void Update(EntityManager entities, in GameTime time)
    {
        _physicsWorld.Step(time.DeltaTime);
        SyncTransforms(entities);
    }

    private void SyncTransforms(EntityManager entities)
    {
        var rbPool = entities.GetPool<RigidbodyComponent>();
        var transformPool = entities.GetPool<TransformComponent>();
        var rbSpan = rbPool.AsSpan();

        for (int i = 0; i < rbSpan.Length; i++)
        {
            ref var rb = ref rbSpan[i];
            if (rb.BodyHandle < 0 || rb.IsKinematic) continue;

            int entityId = rbPool.GetEntityAtIndex(i);
            if (!transformPool.Has(entityId)) continue;

            var (pos, rot) = _physicsWorld.GetBodyPose(new BodyHandle(rb.BodyHandle));

            ref var transform = ref transformPool.Get(entityId);
            transform.Position = pos;
            transform.Rotation = rot;
            transform.IsDirty = true;
        }
    }
}
