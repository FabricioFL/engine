# Logging

The logging system provides leveled logging with console and file output.

## ILogger

Access via DI:

```csharp
var logger = services.GetRequiredService<ILogger>();
```

## Log Levels

| Level | Method | Purpose |
|---|---|---|
| Debug | `logger.Log(LogLevel.Debug, ...)` | Verbose debugging info |
| Info | `logger.Info(category, message)` | General information |
| Warning | `logger.Warning(category, message)` | Potential issues |
| Error | `logger.Error(category, message)` | Errors and failures |
| Build | `logger.Build(category, message)` | Build/startup messages |

## Usage

```csharp
logger.Info("Game", "Player spawned at origin");
logger.Warning("Physics", "Body went to sleep unexpectedly");
logger.Error("Assets", $"Failed to load texture: {path}");
logger.Build("Engine", "Initialization complete");
```

The first parameter is a category string for filtering and grouping.

## Sinks

- **ConsoleSink** — colored output to the terminal
- **FileSink** — writes to files in `logs/` directory

Both sinks are active by default. Configure in `config/log.json`:

```json
{
  "MinLevel": "Info",
  "MaxFileSize": 5242880,
  "EnableConsole": true,
  "EnableFile": true
}
```

## Log Rotation

When a log file exceeds `MaxFileSize`, it is rotated automatically. Old files are renamed with timestamps.

## Output Location

Log files are written to the `logs/` directory at the project root. This directory is gitignored.
