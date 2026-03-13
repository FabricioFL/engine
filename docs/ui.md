# UI System

The UI system uses HTML/CSS-like markup files parsed at runtime, with C# controller classes for interaction. The layout engine supports a flexbox-lite model rendered as an orthographic overlay.

## Architecture

```
ui/
  gui/           # HTML templates + CSS styles
    hud.html
    hud.css
    main-menu.html
    main-menu.css
  control/       # C# controllers
    HudController.cs
    MainMenuController.cs
config/
  ui.json        # View registry
```

## Configuration

Register views in `config/ui.json`:

```json
{
  "Views": [
    {
      "Template": "hud.html",
      "Controller": "HudController",
      "Mountable": true
    },
    {
      "Template": "main-menu.html",
      "Controller": "MainMenuController",
      "Mountable": true
    }
  ]
}
```

## HTML Templates

Templates use a strict HTML subset. Supported tags: `div`, `span`, `button`, `img`, `text`, `input`.

```html
<div id="menu-bg">
    <div id="menu-container">
        <div id="title">MY GAME</div>
        <div id="btn-play" class="menu-btn">Play</div>
        <div id="btn-options" class="menu-btn">Options</div>
        <div id="btn-quit" class="menu-btn">Quit</div>
    </div>
</div>
```

Elements are identified by `id` for controller bindings and styled by `id` or `class` in CSS.

## CSS Styles

CSS files are loaded alongside their HTML template (same name, `.css` extension). Supported properties:

### Layout
| Property | Values | Description |
|---|---|---|
| width | number | Fixed width in pixels |
| height | number | Fixed height in pixels |
| flex-direction | row, column | Layout direction |
| justify-content | start, center, end, space-between | Main axis alignment |
| align-items | start, center, end | Cross axis alignment |
| gap | number | Space between children |
| margin-top/bottom/left/right | number | Outer spacing |
| padding-top/bottom/left/right | number | Inner spacing |
| display | flex, none | Visibility |
| position | relative, absolute | Positioning mode |
| top, left | number | Offset (absolute positioning) |

### Appearance
| Property | Values | Description |
|---|---|---|
| background-color | #RRGGBB or #RRGGBBaa | Background color |
| color | #RRGGBB or #RRGGBBaa | Text color |
| font-size | number | Font size in pixels |

### Example

```css
#menu-bg {
    background-color: #000000cc;
    flex-direction: column;
    justify-content: center;
    align-items: center;
}

#menu-container {
    width: 300;
    height: 400;
    flex-direction: column;
    align-items: center;
    gap: 16;
}

.menu-btn {
    width: 200;
    height: 44;
    background-color: #333333;
    color: #ffffff;
    font-size: 20;
    justify-content: center;
    align-items: center;
}
```

Note: values are plain numbers (pixels), not `px`.

## Controllers

Controllers handle UI logic. Extend `UiControllerBase`:

```csharp
using Engine.UI.Runtime;

public class MainMenuController : UiControllerBase
{
    public Action? OnPlay { get; set; }
    public Action? OnOptions { get; set; }
    public Action? OnQuit { get; set; }

    public override void OnLoad()
    {
        RegisterClick("btn-play", () => OnPlay?.Invoke());
        RegisterClick("btn-options", () => OnOptions?.Invoke());
        RegisterClick("btn-quit", () => OnQuit?.Invoke());
    }
}
```

### Controller API

| Method | Description |
|---|---|
| `OnLoad()` | Called when the view is mounted. Register click handlers here. |
| `OnUnload()` | Called when the view is unmounted. |
| `OnUpdate(in GameTime)` | Called each frame while mounted. |
| `RegisterClick(id, handler)` | Register a click handler on an element by ID. |
| `FindById(id)` | Find a UI node by its ID. |
| `SetText(id, text)` | Update the text content of an element. |
| `SetVisible(id, visible)` | Show or hide an element. |
| `GetAttribute(id, attribute)` | Read an attribute value from an element. |

## Mounting and Unmounting

Mount views from your game code:

```csharp
var ui = services.GetRequiredService<UiManager>();

// Mount with controller
var menuCtrl = new MainMenuController();
menuCtrl.OnPlay = () => StartGame();
menuCtrl.OnQuit = () => QuitGame();
ui.Mount("main-menu.html", menuCtrl);

// Check if mounted
if (ui.IsMounted("main-menu.html")) { }

// Unmount
ui.Unmount("main-menu.html");
```

Multiple views can be mounted simultaneously (e.g., HUD + dialog overlay).

## Updating UI at Runtime

Update text or visibility from your game loop:

```csharp
public class HudController : UiControllerBase
{
    private float _health;

    public void SetHealth(float percent)
    {
        _health = percent;
        SetText("health-text", $"HP: {(int)(percent * 100)}%");
    }

    public override void OnUpdate(in GameTime time)
    {
        // Update every frame if needed
    }

    public override void OnLoad()
    {
        RegisterClick("btn-pause", () => OnPause?.Invoke());
    }
}
```

## Screen Resize

The UI re-layouts automatically when the window is resized. The engine calls `UiManager.UpdateScreenSize()` on both window resize and framebuffer resize events.

## Click Handling

The engine routes mouse clicks to mounted views automatically. Click handlers registered with `RegisterClick` fire when the user clicks within the element's computed bounds.
