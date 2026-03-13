using Engine.Config;
using Engine.Core;
using Engine.Logging;
using Engine.UI.Layout;
using System.Numerics;
using Engine.UI.Markup;

namespace Engine.UI.Runtime;

public sealed class UiManager
{
    private readonly UiConfig _config;
    private readonly ILogger _logger;
    private readonly Dictionary<string, UiView> _views = new();
    private readonly Dictionary<string, UiView> _mountedViews = new();
    private float _screenWidth;
    private float _screenHeight;

    public UiManager(UiConfig config, ILogger logger)
    {
        _config = config;
        _logger = logger;
    }

    public void Initialize(float screenWidth, float screenHeight, string basePath)
    {
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;

        foreach (var entry in _config.Views)
        {
            string templatePath = Path.Combine(basePath, "ui", "gui", entry.Template);
            if (!File.Exists(templatePath))
            {
                _logger.Warning("UiManager", $"Template not found: {templatePath}");
                continue;
            }

            string html = File.ReadAllText(templatePath);
            var rootNode = HtmlParser.Parse(html);

            // Load associated CSS if present
            string cssPath = Path.ChangeExtension(templatePath, ".css");
            if (File.Exists(cssPath))
            {
                string css = File.ReadAllText(cssPath);
                var styles = CssParser.Parse(css);
                ApplyStyles(rootNode, styles);
            }

            // Load common styles
            string commonCssPath = Path.Combine(basePath, "ui", "gui", "styles", "common.css");
            if (File.Exists(commonCssPath))
            {
                string css = File.ReadAllText(commonCssPath);
                var styles = CssParser.Parse(css);
                ApplyStyles(rootNode, styles);
            }

            var view = new UiView(entry.Template, rootNode, entry.Mountable);
            _views[entry.Template] = view;

            _logger.Info("UiManager", $"Loaded UI template: {entry.Template}");
        }
    }

    public void Mount(string templateName, UiControllerBase controller)
    {
        if (!_views.TryGetValue(templateName, out var view))
        {
            _logger.Error("UiManager", $"View '{templateName}' not found");
            return;
        }

        if (!view.IsMountable)
        {
            _logger.Error("UiManager", $"View '{templateName}' is not mountable");
            return;
        }

        controller.RootNode = view.RootNode;
        view.Controller = controller;
        _mountedViews[templateName] = view;

        LayoutEngine.ComputeLayout(view.RootNode, _screenWidth, _screenHeight);
        controller.OnLoad();

        _logger.Info("UiManager", $"Mounted UI: {templateName}");
    }

    public void Unmount(string templateName)
    {
        if (_mountedViews.TryGetValue(templateName, out var view))
        {
            view.Controller?.OnUnload();
            view.Controller = null;
            _mountedViews.Remove(templateName);
        }
    }

    public bool IsMounted(string templateName) => _mountedViews.ContainsKey(templateName);

    public void UpdateScreenSize(float width, float height)
    {
        _screenWidth = width;
        _screenHeight = height;

        // Re-layout all mounted views
        foreach (var view in _mountedViews.Values)
        {
            LayoutEngine.ComputeLayout(view.RootNode, _screenWidth, _screenHeight);
        }
    }

    public void Update(in GameTime time)
    {
        foreach (var view in _mountedViews.Values)
        {
            view.Controller?.OnUpdate(in time);
        }
    }

    public bool HandleClick(Vector2 mousePos)
    {
        // Check mounted views in reverse order (last mounted = on top)
        foreach (var view in _mountedViews.Values.Reverse())
        {
            if (view.Controller != null && view.Controller.HandleClick(mousePos))
                return true;
        }
        return false;
    }

    public IEnumerable<UiView> GetMountedViews() => _mountedViews.Values;

    private static void ApplyStyles(UiNode node, Dictionary<string, UiStyle> styles)
    {
        // Apply by tag name
        if (styles.TryGetValue(node.Tag, out var tagStyle))
            MergeStyle(node.Style, tagStyle);

        // Apply by class
        foreach (var cls in node.Classes)
        {
            if (styles.TryGetValue($".{cls}", out var classStyle))
                MergeStyle(node.Style, classStyle);
        }

        // Apply by id
        if (node.Id != null && styles.TryGetValue($"#{node.Id}", out var idStyle))
            MergeStyle(node.Style, idStyle);

        foreach (var child in node.Children)
            ApplyStyles(child, styles);
    }

    private static void MergeStyle(UiStyle target, UiStyle source)
    {
        if (source.BackgroundColor != Vector4.Zero) target.BackgroundColor = source.BackgroundColor;
        if (source.TextColor != Vector4.One) target.TextColor = source.TextColor;
        if (source.FontSize != 16) target.FontSize = source.FontSize;
        if (source.Width.HasValue) target.Width = source.Width;
        if (source.Height.HasValue) target.Height = source.Height;
        if (source.MarginTop != 0) target.MarginTop = source.MarginTop;
        if (source.MarginBottom != 0) target.MarginBottom = source.MarginBottom;
        if (source.MarginLeft != 0) target.MarginLeft = source.MarginLeft;
        if (source.MarginRight != 0) target.MarginRight = source.MarginRight;
        if (source.PaddingTop != 0) target.PaddingTop = source.PaddingTop;
        if (source.PaddingBottom != 0) target.PaddingBottom = source.PaddingBottom;
        if (source.PaddingLeft != 0) target.PaddingLeft = source.PaddingLeft;
        if (source.PaddingRight != 0) target.PaddingRight = source.PaddingRight;
        if (source.Gap != 0) target.Gap = source.Gap;
        target.FlexDirection = source.FlexDirection;
        target.JustifyContent = source.JustifyContent;
        target.AlignItems = source.AlignItems;
        target.Display = source.Display;
        target.Position = source.Position;
        if (source.Top.HasValue) target.Top = source.Top;
        if (source.Left.HasValue) target.Left = source.Left;
    }

}
