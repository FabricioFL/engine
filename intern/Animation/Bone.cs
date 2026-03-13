using System.Numerics;

namespace Engine.Animation;

public struct Bone
{
    public int Index;
    public int ParentIndex;       // -1 for root
    public int NameHash;          // Precomputed hash for fast lookup
    public string Name;
    public Matrix4x4 BindPose;    // Local bind pose
    public Matrix4x4 InverseBindPose;

    public bool IsRoot => ParentIndex < 0;
}
