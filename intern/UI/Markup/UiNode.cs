using System.Numerics;

namespace Engine.UI.Markup;

public sealed class UiNode
{
    public string Tag { get; set; } = "div";
    public string? Id { get; set; }
    public List<string> Classes { get; } = new();
    public Dictionary<string, string> Attributes { get; } = new();
    public string? TextContent { get; set; }
    public List<UiNode> Children { get; } = new();
    public UiNode? Parent { get; set; }

    // Resolved styles
    public UiStyle Style { get; set; } = new();

    // Layout results
    public Vector2 ComputedPosition { get; set; }
    public Vector2 ComputedSize { get; set; }
}

public sealed class UiStyle
{
    public Vector4 BackgroundColor { get; set; } = Vector4.Zero;    // RGBA
    public Vector4 TextColor { get; set; } = Vector4.One;
    public float FontSize { get; set; } = 16;
    public float MarginTop { get; set; }
    public float MarginBottom { get; set; }
    public float MarginLeft { get; set; }
    public float MarginRight { get; set; }
    public float PaddingTop { get; set; }
    public float PaddingBottom { get; set; }
    public float PaddingLeft { get; set; }
    public float PaddingRight { get; set; }
    public float? Width { get; set; }
    public float? Height { get; set; }
    public FlexDirection FlexDirection { get; set; } = FlexDirection.Column;
    public FlexAlign JustifyContent { get; set; } = FlexAlign.Start;
    public FlexAlign AlignItems { get; set; } = FlexAlign.Start;
    public float Gap { get; set; }
    public DisplayMode Display { get; set; } = DisplayMode.Flex;
    public PositionMode Position { get; set; } = PositionMode.Relative;
    public float? Top { get; set; }
    public float? Left { get; set; }
}

public enum FlexDirection { Row, Column }
public enum FlexAlign { Start, Center, End, SpaceBetween }
public enum DisplayMode { Flex, None }
public enum PositionMode { Relative, Absolute }
