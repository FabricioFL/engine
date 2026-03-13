namespace Engine.ECS.Components;

public struct ScriptComponent : IComponent
{
    public int ScriptIndex;

    public ScriptComponent(int scriptIndex)
    {
        ScriptIndex = scriptIndex;
    }
}
