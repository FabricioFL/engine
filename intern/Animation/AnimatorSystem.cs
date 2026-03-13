using System.Numerics;
using Engine.Core;
using Engine.ECS;
using Engine.ECS.Components;

namespace Engine.Animation;

public sealed class AnimatorSystem : SystemBase
{
    private readonly List<AnimationClip> _clips = new();
    private readonly List<Skeleton> _skeletons = new();

    public override int Priority => 50; // After physics, before rendering

    public int RegisterClip(AnimationClip clip)
    {
        _clips.Add(clip);
        return _clips.Count - 1;
    }

    public int RegisterSkeleton(Skeleton skeleton)
    {
        _skeletons.Add(skeleton);
        return _skeletons.Count - 1;
    }

    public AnimationClip? GetClip(int index) =>
        index >= 0 && index < _clips.Count ? _clips[index] : null;

    public Skeleton? GetSkeleton(int index) =>
        index >= 0 && index < _skeletons.Count ? _skeletons[index] : null;

    public override void Update(EntityManager entities, in GameTime time)
    {
        var skeletonPool = entities.GetPool<SkeletonComponent>();
        var span = skeletonPool.AsSpan();

        for (int i = 0; i < span.Length; i++)
        {
            ref var comp = ref span[i];
            if (comp.SkeletonHandle < 0 || comp.SkeletonHandle >= _skeletons.Count)
                continue;

            var skeleton = _skeletons[comp.SkeletonHandle];

            if (comp.AnimationStateIndex >= 0)
            {
                // Animation-driven: evaluate clip at current time
                // This is placeholder — actual state management would be here
            }

            // Compute world transforms from local transforms
            if (comp.BoneLocalTransforms != null && comp.BoneWorldTransforms != null)
            {
                skeleton.ComputeWorldTransforms(comp.BoneLocalTransforms, comp.BoneWorldTransforms);
            }
        }
    }

    public void PlayClip(ref SkeletonComponent comp, int clipIndex, bool loop = true)
    {
        if (clipIndex < 0 || clipIndex >= _clips.Count) return;
        comp.AnimationStateIndex = clipIndex;

        var skeleton = _skeletons[comp.SkeletonHandle];
        var clip = _clips[clipIndex];

        // Reset to bind pose then apply first frame
        for (int b = 0; b < skeleton.BoneCount; b++)
        {
            var kf = clip.Sample(b, 0);
            comp.BoneLocalTransforms[b] =
                Matrix4x4.CreateScale(kf.Scale) *
                Matrix4x4.CreateFromQuaternion(kf.Rotation) *
                Matrix4x4.CreateTranslation(kf.Position);
        }
    }

    public void EvaluateClip(ref SkeletonComponent comp, int clipIndex, float time)
    {
        if (clipIndex < 0 || clipIndex >= _clips.Count) return;
        if (comp.SkeletonHandle < 0 || comp.SkeletonHandle >= _skeletons.Count) return;

        var skeleton = _skeletons[comp.SkeletonHandle];
        var clip = _clips[clipIndex];

        for (int b = 0; b < skeleton.BoneCount; b++)
        {
            var kf = clip.Sample(b, time);
            comp.BoneLocalTransforms[b] =
                Matrix4x4.CreateScale(kf.Scale) *
                Matrix4x4.CreateFromQuaternion(kf.Rotation) *
                Matrix4x4.CreateTranslation(kf.Position);
        }
    }
}
