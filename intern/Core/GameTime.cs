namespace Engine.Core;

public readonly struct GameTime
{
    public readonly float DeltaTime;
    public readonly double TotalTime;
    public readonly long FrameCount;

    public GameTime(float deltaTime, double totalTime, long frameCount)
    {
        DeltaTime = deltaTime;
        TotalTime = totalTime;
        FrameCount = frameCount;
    }
}
