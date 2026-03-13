using System.Numerics;
using Engine.UI.Markup;

namespace Engine.UI.Layout;

public static class LayoutEngine
{
    public static void ComputeLayout(UiNode root, float screenWidth, float screenHeight)
    {
        root.ComputedPosition = Vector2.Zero;
        root.ComputedSize = new Vector2(screenWidth, screenHeight);
        LayoutChildren(root);
    }

    private static void LayoutChildren(UiNode node)
    {
        var style = node.Style;

        if (style.Display == DisplayMode.None)
            return;

        float contentX = node.ComputedPosition.X + style.PaddingLeft + style.MarginLeft;
        float contentY = node.ComputedPosition.Y + style.PaddingTop + style.MarginTop;
        float contentW = node.ComputedSize.X - style.PaddingLeft - style.PaddingRight - style.MarginLeft - style.MarginRight;
        float contentH = node.ComputedSize.Y - style.PaddingTop - style.PaddingBottom - style.MarginTop - style.MarginBottom;

        var visibleChildren = new List<UiNode>();
        foreach (var child in node.Children)
        {
            if (child.Style.Display != DisplayMode.None)
                visibleChildren.Add(child);
        }

        if (visibleChildren.Count == 0) return;

        bool isRow = style.FlexDirection == FlexDirection.Row;
        float totalGap = style.Gap * (visibleChildren.Count - 1);
        float availableMain = (isRow ? contentW : contentH) - totalGap;

        // Size children
        float mainPerChild = availableMain / visibleChildren.Count;
        foreach (var child in visibleChildren)
        {
            float w = child.Style.Width ?? (isRow ? mainPerChild : contentW);
            float h = child.Style.Height ?? (isRow ? contentH : mainPerChild);
            child.ComputedSize = new Vector2(w, h);
        }

        // Position children
        float offset = 0;

        if (style.JustifyContent == FlexAlign.Center)
        {
            float totalMain = 0;
            foreach (var child in visibleChildren)
                totalMain += isRow ? child.ComputedSize.X : child.ComputedSize.Y;
            totalMain += totalGap;
            offset = ((isRow ? contentW : contentH) - totalMain) / 2f;
        }
        else if (style.JustifyContent == FlexAlign.End)
        {
            float totalMain = 0;
            foreach (var child in visibleChildren)
                totalMain += isRow ? child.ComputedSize.X : child.ComputedSize.Y;
            totalMain += totalGap;
            offset = (isRow ? contentW : contentH) - totalMain;
        }

        foreach (var child in visibleChildren)
        {
            if (child.Style.Position == PositionMode.Absolute)
            {
                child.ComputedPosition = new Vector2(
                    node.ComputedPosition.X + (child.Style.Left ?? 0),
                    node.ComputedPosition.Y + (child.Style.Top ?? 0));
                LayoutChildren(child);
                continue;
            }

            float crossOffset = 0;
            float childCross = isRow ? child.ComputedSize.Y : child.ComputedSize.X;
            float parentCross = isRow ? contentH : contentW;

            if (style.AlignItems == FlexAlign.Center)
                crossOffset = (parentCross - childCross) / 2f;
            else if (style.AlignItems == FlexAlign.End)
                crossOffset = parentCross - childCross;

            if (isRow)
            {
                child.ComputedPosition = new Vector2(contentX + offset, contentY + crossOffset);
                offset += child.ComputedSize.X + style.Gap;
            }
            else
            {
                child.ComputedPosition = new Vector2(contentX + crossOffset, contentY + offset);
                offset += child.ComputedSize.Y + style.Gap;
            }

            LayoutChildren(child);
        }
    }
}
