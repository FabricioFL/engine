using System.Globalization;
using System.Numerics;

namespace Engine.UI.Markup;

public static class CssParser
{
    public static Dictionary<string, UiStyle> Parse(string css)
    {
        var styles = new Dictionary<string, UiStyle>();
        int pos = 0;

        while (pos < css.Length)
        {
            SkipWhitespace(css, ref pos);
            if (pos >= css.Length) break;

            // Read selector
            int selStart = pos;
            while (pos < css.Length && css[pos] != '{')
                pos++;

            string selector = css[selStart..pos].Trim();
            if (pos < css.Length) pos++; // skip '{'

            // Read properties
            int propStart = pos;
            while (pos < css.Length && css[pos] != '}')
                pos++;

            string propsBlock = css[propStart..pos];
            if (pos < css.Length) pos++; // skip '}'

            var style = ParseProperties(propsBlock);
            styles[selector] = style;
        }

        return styles;
    }

    private static UiStyle ParseProperties(string block)
    {
        var style = new UiStyle();
        var props = block.Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (var prop in props)
        {
            var parts = prop.Split(':', 2);
            if (parts.Length != 2) continue;

            string name = parts[0].Trim().ToLowerInvariant();
            string value = parts[1].Trim();

            switch (name)
            {
                case "background-color": style.BackgroundColor = ParseColor(value); break;
                case "color": style.TextColor = ParseColor(value); break;
                case "font-size": style.FontSize = ParsePixels(value); break;
                case "width": style.Width = ParsePixels(value); break;
                case "height": style.Height = ParsePixels(value); break;
                case "margin-top": style.MarginTop = ParsePixels(value); break;
                case "margin-bottom": style.MarginBottom = ParsePixels(value); break;
                case "margin-left": style.MarginLeft = ParsePixels(value); break;
                case "margin-right": style.MarginRight = ParsePixels(value); break;
                case "padding-top": style.PaddingTop = ParsePixels(value); break;
                case "padding-bottom": style.PaddingBottom = ParsePixels(value); break;
                case "padding-left": style.PaddingLeft = ParsePixels(value); break;
                case "padding-right": style.PaddingRight = ParsePixels(value); break;
                case "margin":
                    float m = ParsePixels(value);
                    style.MarginTop = style.MarginBottom = style.MarginLeft = style.MarginRight = m;
                    break;
                case "padding":
                    float p = ParsePixels(value);
                    style.PaddingTop = style.PaddingBottom = style.PaddingLeft = style.PaddingRight = p;
                    break;
                case "flex-direction":
                    style.FlexDirection = value == "row" ? FlexDirection.Row : FlexDirection.Column;
                    break;
                case "justify-content":
                    style.JustifyContent = ParseAlign(value);
                    break;
                case "align-items":
                    style.AlignItems = ParseAlign(value);
                    break;
                case "gap":
                    style.Gap = ParsePixels(value);
                    break;
                case "display":
                    style.Display = value == "none" ? DisplayMode.None : DisplayMode.Flex;
                    break;
                case "position":
                    style.Position = value == "absolute" ? PositionMode.Absolute : PositionMode.Relative;
                    break;
                case "top": style.Top = ParsePixels(value); break;
                case "left": style.Left = ParsePixels(value); break;
            }
        }

        return style;
    }

    private static float ParsePixels(string value)
    {
        value = value.Replace("px", "").Trim();
        return float.TryParse(value, CultureInfo.InvariantCulture, out float result) ? result : 0;
    }

    private static FlexAlign ParseAlign(string value) => value switch
    {
        "center" => FlexAlign.Center,
        "flex-end" or "end" => FlexAlign.End,
        "space-between" => FlexAlign.SpaceBetween,
        _ => FlexAlign.Start
    };

    private static Vector4 ParseColor(string value)
    {
        value = value.Trim();

        if (value.StartsWith('#'))
        {
            string hex = value[1..];
            if (hex.Length == 6)
            {
                int r = Convert.ToInt32(hex[..2], 16);
                int g = Convert.ToInt32(hex[2..4], 16);
                int b = Convert.ToInt32(hex[4..6], 16);
                return new Vector4(r / 255f, g / 255f, b / 255f, 1f);
            }
            if (hex.Length == 8)
            {
                int r = Convert.ToInt32(hex[..2], 16);
                int g = Convert.ToInt32(hex[2..4], 16);
                int b = Convert.ToInt32(hex[4..6], 16);
                int a = Convert.ToInt32(hex[6..8], 16);
                return new Vector4(r / 255f, g / 255f, b / 255f, a / 255f);
            }
        }

        return value switch
        {
            "white" => Vector4.One,
            "black" => new Vector4(0, 0, 0, 1),
            "red" => new Vector4(1, 0, 0, 1),
            "green" => new Vector4(0, 1, 0, 1),
            "blue" => new Vector4(0, 0, 1, 1),
            "transparent" => Vector4.Zero,
            _ => Vector4.One
        };
    }

    private static void SkipWhitespace(string s, ref int pos)
    {
        while (pos < s.Length && char.IsWhiteSpace(s[pos]))
            pos++;
    }
}
