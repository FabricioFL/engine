using System.Numerics;

namespace Engine.Animation;

public sealed class AnimationClip
{
    public string Name { get; }
    public float Duration { get; }
    public bool Loop { get; set; } = true;

    // Flat array: [bone0_key0, bone0_key1, ..., bone1_key0, bone1_key1, ...]
    private readonly Keyframe[] _keyframes;
    private readonly int[] _boneKeyframeOffsets;
    private readonly int[] _boneKeyframeCounts;
    private readonly int _boneCount;

    public AnimationClip(string name, float duration, int boneCount)
    {
        Name = name;
        Duration = duration;
        _boneCount = boneCount;
        _boneKeyframeOffsets = new int[boneCount];
        _boneKeyframeCounts = new int[boneCount];
        _keyframes = Array.Empty<Keyframe>();
    }

    public AnimationClip(string name, float duration, int boneCount,
        Keyframe[] keyframes, int[] offsets, int[] counts)
    {
        Name = name;
        Duration = duration;
        _boneCount = boneCount;
        _keyframes = keyframes;
        _boneKeyframeOffsets = offsets;
        _boneKeyframeCounts = counts;
    }

    public Keyframe Sample(int boneIndex, float time)
    {
        if (boneIndex < 0 || boneIndex >= _boneCount)
            return new Keyframe(time, Vector3.Zero, Quaternion.Identity, Vector3.One);

        int offset = _boneKeyframeOffsets[boneIndex];
        int count = _boneKeyframeCounts[boneIndex];

        if (count == 0)
            return new Keyframe(time, Vector3.Zero, Quaternion.Identity, Vector3.One);

        if (count == 1)
            return _keyframes[offset];

        // Find surrounding keyframes
        for (int i = 0; i < count - 1; i++)
        {
            ref var a = ref _keyframes[offset + i];
            ref var b = ref _keyframes[offset + i + 1];

            if (time >= a.Time && time <= b.Time)
            {
                float t = (time - a.Time) / (b.Time - a.Time);
                return Keyframe.Lerp(in a, in b, t);
            }
        }

        return _keyframes[offset + count - 1];
    }

    // Builder for creating clips procedurally
    public static AnimationClipBuilder CreateBuilder(string name, float duration, int boneCount)
    {
        return new AnimationClipBuilder(name, duration, boneCount);
    }
}

public sealed class AnimationClipBuilder
{
    private readonly string _name;
    private readonly float _duration;
    private readonly int _boneCount;
    private readonly List<List<Keyframe>> _boneKeyframes;

    public AnimationClipBuilder(string name, float duration, int boneCount)
    {
        _name = name;
        _duration = duration;
        _boneCount = boneCount;
        _boneKeyframes = new List<List<Keyframe>>(boneCount);
        for (int i = 0; i < boneCount; i++)
            _boneKeyframes.Add(new List<Keyframe>());
    }

    public AnimationClipBuilder AddKeyframe(int boneIndex, float time,
        Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (boneIndex >= 0 && boneIndex < _boneCount)
            _boneKeyframes[boneIndex].Add(new Keyframe(time, position, rotation, scale));
        return this;
    }

    public AnimationClipBuilder AddKeyframe(int boneIndex, Keyframe keyframe)
    {
        if (boneIndex >= 0 && boneIndex < _boneCount)
            _boneKeyframes[boneIndex].Add(keyframe);
        return this;
    }

    public AnimationClip Build()
    {
        int totalKeyframes = 0;
        for (int i = 0; i < _boneCount; i++)
            totalKeyframes += _boneKeyframes[i].Count;

        var keyframes = new Keyframe[totalKeyframes];
        var offsets = new int[_boneCount];
        var counts = new int[_boneCount];

        int currentOffset = 0;
        for (int i = 0; i < _boneCount; i++)
        {
            var boneKeys = _boneKeyframes[i];
            boneKeys.Sort((a, b) => a.Time.CompareTo(b.Time));

            offsets[i] = currentOffset;
            counts[i] = boneKeys.Count;

            for (int j = 0; j < boneKeys.Count; j++)
                keyframes[currentOffset + j] = boneKeys[j];

            currentOffset += boneKeys.Count;
        }

        return new AnimationClip(_name, _duration, _boneCount, keyframes, offsets, counts);
    }
}
