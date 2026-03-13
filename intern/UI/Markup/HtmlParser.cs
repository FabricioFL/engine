namespace Engine.UI.Markup;

public static class HtmlParser
{
    private static readonly HashSet<string> AllowedTags = new()
    {
        "div", "span", "button", "img", "text", "input", "label"
    };

    public static UiNode Parse(string html)
    {
        var root = new UiNode { Tag = "root" };
        int pos = 0;
        ParseChildren(html, ref pos, root);
        return root;
    }

    private static void ParseChildren(string html, ref int pos, UiNode parent)
    {
        while (pos < html.Length)
        {
            SkipWhitespace(html, ref pos);
            if (pos >= html.Length) break;

            if (html[pos] == '<')
            {
                if (pos + 1 < html.Length && html[pos + 1] == '/')
                    break; // Closing tag — return to parent

                var node = ParseElement(html, ref pos);
                if (node != null)
                {
                    node.Parent = parent;
                    parent.Children.Add(node);
                }
            }
            else
            {
                // Text content
                int start = pos;
                while (pos < html.Length && html[pos] != '<')
                    pos++;

                string text = html[start..pos].Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    parent.Children.Add(new UiNode
                    {
                        Tag = "text",
                        TextContent = text,
                        Parent = parent
                    });
                }
            }
        }
    }

    private static UiNode? ParseElement(string html, ref int pos)
    {
        pos++; // skip '<'
        SkipWhitespace(html, ref pos);

        // Read tag name
        int tagStart = pos;
        while (pos < html.Length && html[pos] != ' ' && html[pos] != '>' && html[pos] != '/')
            pos++;

        string tag = html[tagStart..pos].ToLowerInvariant();

        if (!AllowedTags.Contains(tag))
        {
            // Skip unknown tag
            while (pos < html.Length && html[pos] != '>')
                pos++;
            if (pos < html.Length) pos++;
            return null;
        }

        var node = new UiNode { Tag = tag };

        // Parse attributes
        while (pos < html.Length && html[pos] != '>' && html[pos] != '/')
        {
            SkipWhitespace(html, ref pos);
            if (pos >= html.Length || html[pos] == '>' || html[pos] == '/') break;

            var (name, value) = ParseAttribute(html, ref pos);
            if (name == "id") node.Id = value;
            else if (name == "class")
            {
                foreach (var cls in value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    node.Classes.Add(cls);
            }
            else
                node.Attributes[name] = value;
        }

        // Self-closing tag
        if (pos < html.Length && html[pos] == '/')
        {
            pos++;
            if (pos < html.Length && html[pos] == '>') pos++;
            return node;
        }

        if (pos < html.Length && html[pos] == '>') pos++;

        // Parse children
        ParseChildren(html, ref pos, node);

        // Skip closing tag
        if (pos < html.Length && html[pos] == '<' && pos + 1 < html.Length && html[pos + 1] == '/')
        {
            while (pos < html.Length && html[pos] != '>')
                pos++;
            if (pos < html.Length) pos++;
        }

        return node;
    }

    private static (string name, string value) ParseAttribute(string html, ref int pos)
    {
        int nameStart = pos;
        while (pos < html.Length && html[pos] != '=' && html[pos] != ' ' && html[pos] != '>' && html[pos] != '/')
            pos++;

        string name = html[nameStart..pos].Trim();

        if (pos >= html.Length || html[pos] != '=')
            return (name, "true");

        pos++; // skip '='

        char quote = '"';
        if (pos < html.Length && (html[pos] == '"' || html[pos] == '\''))
        {
            quote = html[pos];
            pos++;
        }

        int valueStart = pos;
        while (pos < html.Length && html[pos] != quote)
            pos++;

        string value = html[valueStart..pos];
        if (pos < html.Length) pos++; // skip closing quote

        return (name, value);
    }

    private static void SkipWhitespace(string html, ref int pos)
    {
        while (pos < html.Length && char.IsWhiteSpace(html[pos]))
            pos++;
    }
}
