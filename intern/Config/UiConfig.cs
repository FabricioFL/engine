namespace Engine.Config;

public sealed class UiConfig
{
    public List<UiViewEntry> Views { get; set; } = new();
}

public sealed class UiViewEntry
{
    public string Template { get; set; } = string.Empty;
    public string Controller { get; set; } = string.Empty;
    public bool Mountable { get; set; } = true;
}
