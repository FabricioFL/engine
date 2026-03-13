using System.Numerics;
using System.Text.Json;

namespace Engine.Animation;

public sealed class Rig
{
    public string Name { get; }
    public Skeleton Skeleton { get; }

    public Rig(string name, Skeleton skeleton)
    {
        Name = name;
        Skeleton = skeleton;
    }

    public static Rig LoadFromJson(string path)
    {
        var json = File.ReadAllText(path);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        string name = root.GetProperty("name").GetString() ?? "unnamed";
        var bonesArray = root.GetProperty("bones");
        var bones = new Bone[bonesArray.GetArrayLength()];

        for (int i = 0; i < bones.Length; i++)
        {
            var b = bonesArray[i];
            string boneName = b.GetProperty("name").GetString() ?? $"bone_{i}";

            var bindPose = Matrix4x4.Identity;
            if (b.TryGetProperty("position", out var pos))
            {
                bindPose = Matrix4x4.CreateTranslation(
                    pos[0].GetSingle(), pos[1].GetSingle(), pos[2].GetSingle());
            }

            if (b.TryGetProperty("rotation", out var rot))
            {
                var quat = new Quaternion(
                    rot[0].GetSingle(), rot[1].GetSingle(),
                    rot[2].GetSingle(), rot[3].GetSingle());
                bindPose = Matrix4x4.CreateFromQuaternion(quat) * bindPose;
            }

            Matrix4x4.Invert(bindPose, out var inverseBind);

            bones[i] = new Bone
            {
                Index = i,
                ParentIndex = b.GetProperty("parent").GetInt32(),
                NameHash = boneName.GetHashCode(),
                Name = boneName,
                BindPose = bindPose,
                InverseBindPose = inverseBind
            };
        }

        return new Rig(name, new Skeleton(bones));
    }
}
