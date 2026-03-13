using Engine.UI.Markup;

namespace Engine.UI.Runtime;

public sealed class UiView
{
    public string TemplateName { get; }
    public UiNode RootNode { get; }
    public bool IsMountable { get; }
    public UiControllerBase? Controller { get; set; }

    public UiView(string templateName, UiNode rootNode, bool isMountable)
    {
        TemplateName = templateName;
        RootNode = rootNode;
        IsMountable = isMountable;
    }
}
