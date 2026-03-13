namespace Engine.Config;

public sealed class SceneConfig
{
    public List<SceneEntry> Scenes { get; set; } = new();
    public string DefaultScene { get; set; } = string.Empty;
}

public sealed class SceneEntry
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int Order { get; set; }
}
