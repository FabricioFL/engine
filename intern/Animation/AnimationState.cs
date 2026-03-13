namespace Engine.Animation;

public struct AnimationState
{
    public int ClipIndex;       // Index into registered clips
    public float CurrentTime;
    public float Speed;
    public bool Playing;
    public bool Loop;
    public float BlendWeight;

    public static AnimationState Default => new()
    {
        ClipIndex = -1,
        CurrentTime = 0,
        Speed = 1.0f,
        Playing = false,
        Loop = true,
        BlendWeight = 1.0f
    };
}
