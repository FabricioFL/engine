# Skeleton & Animation

The animation system supports bone hierarchies, keyframed animation clips, and procedural animation via code. It is designed for math-driven animations on rigged 2D sprites or 3D meshes.

## Concepts

- **Bone** — a joint in the hierarchy with a bind pose matrix
- **Skeleton** — a tree of bones defining a character's structure
- **AnimationClip** — keyframe data per bone (position, rotation, scale over time)
- **AnimationState** — runtime playback state (current clip, time, speed, looping)
- **AnimatorSystem** — evaluates clips and computes bone transforms each frame

## Bones

```csharp
// Bones are structs with hierarchy info
Bone bone;
bone.Index;            // Index in skeleton
bone.ParentIndex;      // -1 for root
bone.Name;             // "left_arm"
bone.BindPose;         // Local bind pose matrix
bone.InverseBindPose;  // Inverse for skinning
bone.IsRoot;           // true if ParentIndex == -1
```

## Skeleton

A skeleton defines the bone hierarchy:

```csharp
var skeleton = new Skeleton(bones); // Bone[] array

int boneIdx = skeleton.GetBoneIndex("left_arm");
Matrix4x4[] poseBuffer = skeleton.CreatePoseBuffer();

// Compute world-space transforms from local transforms
skeleton.ComputeWorldTransforms(localTransforms, worldTransforms);

// Compute skinning matrices for GPU
skeleton.ComputeSkinningMatrices(worldTransforms, skinningMatrices);
```

## Animation Clips

Create clips with the builder pattern:

```csharp
var clip = AnimationClip.CreateBuilder("walk", duration: 1.0f, boneCount: skeleton.BoneCount)
    .AddKeyframe(boneIndex: 0, time: 0f,
        position: Vector3.Zero,
        rotation: Quaternion.Identity,
        scale: Vector3.One)
    .AddKeyframe(boneIndex: 0, time: 0.5f,
        position: new Vector3(0, 0.1f, 0),
        rotation: Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 0.2f),
        scale: Vector3.One)
    .AddKeyframe(boneIndex: 0, time: 1.0f,
        position: Vector3.Zero,
        rotation: Quaternion.Identity,
        scale: Vector3.One)
    .Build();

clip.Loop = true;
```

Sample a clip at a specific time:

```csharp
Keyframe frame = clip.Sample(boneIndex: 0, time: 0.3f);
// Interpolated position, rotation, scale
```

## Keyframes

```csharp
Keyframe frame;
frame.Time;       // Timestamp
frame.Position;   // Vector3
frame.Rotation;   // Quaternion
frame.Scale;      // Vector3

// Interpolate between two keyframes
Keyframe result = Keyframe.Lerp(frameA, frameB, t: 0.5f);
```

## AnimatorSystem

The system that evaluates animations each frame. Register clips and skeletons:

```csharp
var animator = services.GetRequiredService<AnimatorSystem>();

int skeletonHandle = animator.RegisterSkeleton(skeleton);
int clipHandle = animator.RegisterClip(walkClip);
```

Play a clip on an entity:

```csharp
ref var skelComp = ref entities.GetComponent<SkeletonComponent>(entity);
animator.PlayClip(ref skelComp, clipHandle, loop: true);
```

Evaluate a clip at a specific time (for previewing or blending):

```csharp
animator.EvaluateClip(ref skelComp, clipHandle, time: 0.5f);
```

## SkeletonComponent

Attach to an entity for animation:

```csharp
entities.AddComponent(entity, new SkeletonComponent
{
    SkeletonHandle = skeletonHandle,
    BoneWorldTransforms = new Matrix4x4[skeleton.BoneCount],
    BoneLocalTransforms = skeleton.CreatePoseBuffer()
});
```

### Procedural Animation

Directly set bone transforms each frame for math-driven animation:

```csharp
ref var skel = ref entities.GetComponent<SkeletonComponent>(entity);

// Swing the left arm using a sine wave
int armBone = skeleton.GetBoneIndex("left_arm");
skel.SetBoneLocalRotation(armBone,
    Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.Sin((float)time.TotalTime) * 0.5f));

// Move a bone to a position
skel.SetBoneLocalPosition(armBone, new Vector3(0, 0.5f, 0));
```

This is the primary animation workflow — define bone movements with math in your update loop rather than baking everything into clip files.

## Workflow

1. Define a bone hierarchy (in code or load from `assets/rigs/`)
2. Create animation clips with keyframes, or animate procedurally each frame
3. Register skeletons and clips with `AnimatorSystem`
4. Add `SkeletonComponent` to entities
5. The system evaluates clips and computes bone world transforms each frame
6. Renderers (sprite or mesh) read bone transforms to deform geometry

For 2D sprite characters, bones control the positions of individual sprite parts (head, torso, arms, legs as separate sprites positioned by bone transforms).
