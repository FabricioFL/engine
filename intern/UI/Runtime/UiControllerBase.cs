using System.Numerics;
using Engine.Core;
using Engine.UI.Markup;

namespace Engine.UI.Runtime;

public abstract class UiControllerBase
{
    internal UiNode? RootNode { get; set; }
    internal IServiceProvider? Services { get; set; }

    private readonly Dictionary<string, Action> _clickHandlers = new();

    public abstract void OnLoad();
    public virtual void OnUnload() { }
    public virtual void OnUpdate(in GameTime time) { }

    protected void RegisterClick(string elementId, Action handler)
    {
        _clickHandlers[elementId] = handler;
    }

    internal bool HandleClick(Vector2 mousePos)
    {
        if (RootNode == null) return false;
        return HitTest(RootNode, mousePos);
    }

    private bool HitTest(UiNode node, Vector2 mousePos)
    {
        if (node.Style.Display == DisplayMode.None)
            return false;

        // Check children first (front-to-back, last child is on top)
        for (int i = node.Children.Count - 1; i >= 0; i--)
        {
            if (HitTest(node.Children[i], mousePos))
                return true;
        }

        // Check this node
        if (node.Id != null && _clickHandlers.ContainsKey(node.Id))
        {
            var pos = node.ComputedPosition;
            var size = node.ComputedSize;

            if (mousePos.X >= pos.X && mousePos.X <= pos.X + size.X &&
                mousePos.Y >= pos.Y && mousePos.Y <= pos.Y + size.Y)
            {
                _clickHandlers[node.Id]();
                return true;
            }
        }

        return false;
    }

    protected UiNode? FindById(string id)
    {
        return FindByIdRecursive(RootNode, id);
    }

    protected void SetText(string id, string text)
    {
        var node = FindById(id);
        if (node != null)
            node.TextContent = text;
    }

    protected void SetVisible(string id, bool visible)
    {
        var node = FindById(id);
        if (node != null)
            node.Style.Display = visible ? DisplayMode.Flex : DisplayMode.None;
    }

    protected string? GetAttribute(string id, string attribute)
    {
        var node = FindById(id);
        return node?.Attributes.GetValueOrDefault(attribute);
    }

    private static UiNode? FindByIdRecursive(UiNode? node, string id)
    {
        if (node == null) return null;
        if (node.Id == id) return node;

        for (int i = 0; i < node.Children.Count; i++)
        {
            var found = FindByIdRecursive(node.Children[i], id);
            if (found != null) return found;
        }
        return null;
    }
}
