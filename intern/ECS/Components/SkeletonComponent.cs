using System.Numerics;

namespace Engine.ECS.Components;

public struct SkeletonComponent : IComponent
{
    public int SkeletonHandle;  // Index into Animation.SkeletonRegistry
    public Matrix4x4[] BoneWorldTransforms;
    public Matrix4x4[] BoneLocalTransforms;
    public int AnimationStateIndex;  // Index into AnimatorSystem state array

    public void SetBoneLocalRotation(int boneIndex, Quaternion rotation)
    {
        if (boneIndex < 0 || BoneLocalTransforms == null || boneIndex >= BoneLocalTransforms.Length)
            return;

        Matrix4x4.Decompose(BoneLocalTransforms[boneIndex], out var scale, out _, out var translation);
        BoneLocalTransforms[boneIndex] = Matrix4x4.CreateScale(scale) *
                                          Matrix4x4.CreateFromQuaternion(rotation) *
                                          Matrix4x4.CreateTranslation(translation);
    }

    public void SetBoneLocalPosition(int boneIndex, Vector3 position)
    {
        if (boneIndex < 0 || BoneLocalTransforms == null || boneIndex >= BoneLocalTransforms.Length)
            return;

        Matrix4x4.Decompose(BoneLocalTransforms[boneIndex], out var scale, out var rotation, out _);
        BoneLocalTransforms[boneIndex] = Matrix4x4.CreateScale(scale) *
                                          Matrix4x4.CreateFromQuaternion(rotation) *
                                          Matrix4x4.CreateTranslation(position);
    }
}
