using System.Numerics;

namespace Engine.ECS.Components;

public struct SpriteComponent : IComponent
{
    public int TextureHandle;
    public int AtlasIndex;
    public Vector2 SpriteSize;
    public Vector2 UvOffset;
    public Vector2 UvScale;
    public BillboardMode Billboard;
    public bool FlipX;
    public bool FlipY;

    public static SpriteComponent Default => new()
    {
        TextureHandle = -1,
        AtlasIndex = 0,
        SpriteSize = Vector2.One,
        UvOffset = Vector2.Zero,
        UvScale = Vector2.One,
        Billboard = BillboardMode.Full,
        FlipX = false,
        FlipY = false
    };
}

public enum BillboardMode
{
    None = 0,
    Full = 1,       // Face camera on all axes
    Vertical = 2    // Only rotate around Y axis
}
