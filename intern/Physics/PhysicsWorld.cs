using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Memory;
using Engine.Logging;

namespace Engine.Physics;

public sealed class PhysicsWorld : IDisposable
{
    private readonly Simulation _simulation;
    private readonly BufferPool _bufferPool;
    private readonly ThreadDispatcher _threadDispatcher;
    private readonly ILogger _logger;

    public Simulation Simulation => _simulation;
    public Vector3 Gravity { get; set; } = new(0, -9.81f, 0);

    public PhysicsWorld(ILogger logger)
    {
        _logger = logger;
        _bufferPool = new BufferPool();
        _threadDispatcher = new ThreadDispatcher(Environment.ProcessorCount > 1 ? Environment.ProcessorCount - 1 : 1);

        var narrowPhaseCallbacks = new NarrowPhaseCallbacks();
        var poseIntegratorCallbacks = new PoseIntegratorCallbacks(Gravity);
        var solveDescription = new SolveDescription(4, 1);

        _simulation = Simulation.Create(_bufferPool, narrowPhaseCallbacks, poseIntegratorCallbacks, solveDescription);

        _logger.Info("Physics", "Physics world initialized");
    }

    public void Step(float deltaTime)
    {
        _simulation.Timestep(deltaTime, _threadDispatcher);
    }

    public BodyHandle AddDynamicBody(Vector3 position, Quaternion rotation, float mass, TypedIndex shape)
    {
        var inertia = new BodyInertia { InverseMass = 1f / mass };
        var bodyDesc = BodyDescription.CreateDynamic(
            new RigidPose(position, rotation),
            inertia,
            shape,
            0.01f);

        return _simulation.Bodies.Add(bodyDesc);
    }

    public StaticHandle AddStaticBody(Vector3 position, Quaternion rotation, TypedIndex shape)
    {
        var staticDesc = new StaticDescription(
            new RigidPose(position, rotation),
            shape);

        return _simulation.Statics.Add(staticDesc);
    }

    public TypedIndex CreateBox(float width, float height, float depth)
    {
        return _simulation.Shapes.Add(new Box(width, height, depth));
    }

    public TypedIndex CreateSphere(float radius)
    {
        return _simulation.Shapes.Add(new Sphere(radius));
    }

    public TypedIndex CreateCapsule(float radius, float length)
    {
        return _simulation.Shapes.Add(new Capsule(radius, length));
    }

    public (Vector3 position, Quaternion rotation) GetBodyPose(BodyHandle handle)
    {
        var bodyRef = _simulation.Bodies[handle];
        return (bodyRef.Pose.Position, bodyRef.Pose.Orientation);
    }

    public void SetBodyVelocity(BodyHandle handle, Vector3 linearVelocity)
    {
        _simulation.Awakener.AwakenBody(handle);
        _simulation.Bodies[handle].Velocity.Linear = linearVelocity;
    }

    public void ApplyImpulse(BodyHandle handle, Vector3 impulse)
    {
        _simulation.Awakener.AwakenBody(handle);
        _simulation.Bodies[handle].ApplyLinearImpulse(impulse);
    }

    public void RemoveBody(BodyHandle handle)
    {
        _simulation.Bodies.Remove(handle);
    }

    public void RemoveStatic(StaticHandle handle)
    {
        _simulation.Statics.Remove(handle);
    }

    public void Dispose()
    {
        _simulation.Dispose();
        _threadDispatcher.Dispose();
        _bufferPool.Clear();
        _logger.Info("Physics", "Physics world disposed");
    }
}

internal struct NarrowPhaseCallbacks : INarrowPhaseCallbacks
{
    public void Initialize(Simulation simulation) { }

    public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
        => a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;

    public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
        => true;

    public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair,
        ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>
    {
        pairMaterial.FrictionCoefficient = 0.5f;
        pairMaterial.MaximumRecoveryVelocity = 2f;
        pairMaterial.SpringSettings = new SpringSettings(30, 1);
        return true;
    }

    public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB,
        ref ConvexContactManifold manifold)
        => true;

    public void Dispose() { }
}

internal struct PoseIntegratorCallbacks : IPoseIntegratorCallbacks
{
    private Vector3 _gravity;
    private Vector<float> _gravityX, _gravityY, _gravityZ;

    public readonly AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;
    public readonly bool AllowSubstepsForUnconstrainedBodies => false;
    public readonly bool IntegrateVelocityForKinematics => false;

    public PoseIntegratorCallbacks(Vector3 gravity)
    {
        _gravity = gravity;
        _gravityX = default;
        _gravityY = default;
        _gravityZ = default;
    }

    public void Initialize(Simulation simulation) { }

    public void PrepareForIntegration(float dt)
    {
        _gravityX = new Vector<float>(_gravity.X * dt);
        _gravityY = new Vector<float>(_gravity.Y * dt);
        _gravityZ = new Vector<float>(_gravity.Z * dt);
    }

    public void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation,
        BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt,
        ref BodyVelocityWide velocity)
    {
        velocity.Linear.X += _gravityX;
        velocity.Linear.Y += _gravityY;
        velocity.Linear.Z += _gravityZ;
    }
}
