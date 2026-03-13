# Configuration

The engine loads JSON config files from the `config/` directory at startup using System.Text.Json source generators (no reflection).

## Config Files

### ui.json

Registers UI views and their controllers:

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

- `Template` — HTML file name in `ui/gui/`
- `Controller` — C# controller class name in `ui/control/`
- `Mountable` — whether the view can be mounted at runtime

### scenes.json

Defines available scenes:

```json
{
  "Scenes": [
    { "Name": "main-menu", "Default": true },
    { "Name": "forest" },
    { "Name": "dungeon" }
  ]
}
```

### skills.json

Registers skill class names:

```json
{
  "Skills": ["Fireball", "Heal", "Dash"]
}
```

### log.json

Logging configuration:

```json
{
  "MinLevel": "Info",
  "MaxFileSize": 5242880,
  "ShowBuild": true,
  "EnableConsole": true,
  "EnableFile": true
}
```

- `MinLevel` — minimum log level: Debug, Info, Warning, Error
- `MaxFileSize` — max log file size in bytes before rotation
- `ShowBuild` — show build-level messages
- `EnableConsole` / `EnableFile` — toggle log sinks

### assets.json

Asset loading configuration:

```json
{
  "Folders": ["assets/textures", "assets/models"],
  "Files": ["assets/special/custom.png"]
}
```

## Loading Configs

Configs are loaded automatically by the engine and registered in DI:

```csharp
var uiConfig = services.GetRequiredService<UiConfig>();
var logConfig = services.GetRequiredService<LogConfig>();
```

## Custom Config Loading

Use `ConfigLoader` for custom configs:

```csharp
var myConfig = ConfigLoader.Load<MyConfig>("config/my-config.json");
```

All paths are relative to the project root.
