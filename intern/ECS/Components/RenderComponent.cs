namespace Engine.ECS.Components;

public struct RenderComponent : IComponent
{
    public int MeshHandle;
    public int MaterialHandle;
    public bool Visible;
    public bool CastShadow;

    public static RenderComponent Default => new()
    {
        MeshHandle = -1,
        MaterialHandle = -1,
        Visible = true,
        CastShadow = true
    };
}
