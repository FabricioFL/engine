using System.Numerics;

namespace Engine.Animation;

public sealed class Skeleton
{
    public Bone[] Bones { get; }
    public int BoneCount => Bones.Length;

    private readonly Dictionary<int, int> _nameHashToIndex;

    public Skeleton(Bone[] bones)
    {
        Bones = bones;
        _nameHashToIndex = new Dictionary<int, int>(bones.Length);
        for (int i = 0; i < bones.Length; i++)
            _nameHashToIndex[bones[i].NameHash] = i;
    }

    public int GetBoneIndex(string name)
    {
        int hash = name.GetHashCode();
        return _nameHashToIndex.TryGetValue(hash, out int index) ? index : -1;
    }

    public Matrix4x4[] CreatePoseBuffer()
    {
        var buffer = new Matrix4x4[BoneCount];
        for (int i = 0; i < BoneCount; i++)
            buffer[i] = Bones[i].BindPose;
        return buffer;
    }

    public void ComputeWorldTransforms(Matrix4x4[] localTransforms, Matrix4x4[] worldTransforms)
    {
        for (int i = 0; i < BoneCount; i++)
        {
            if (Bones[i].IsRoot)
            {
                worldTransforms[i] = localTransforms[i];
            }
            else
            {
                worldTransforms[i] = localTransforms[i] * worldTransforms[Bones[i].ParentIndex];
            }
        }
    }

    public void ComputeSkinningMatrices(Matrix4x4[] worldTransforms, Matrix4x4[] skinningMatrices)
    {
        for (int i = 0; i < BoneCount; i++)
            skinningMatrices[i] = Bones[i].InverseBindPose * worldTransforms[i];
    }
}
