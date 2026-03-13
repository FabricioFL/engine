using System.Runtime.CompilerServices;
using Engine.Config;

namespace Engine.Logging;

public sealed class Logger : ILogger, IDisposable
{
    private readonly LogSink[] _sinks;
    private readonly LogConfig _config;

    public Logger(LogConfig config)
    {
        _config = config;
        _sinks = new LogSink[]
        {
            new FileSink(config),
            new ConsoleSink()
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Log(LogLevel level, string category, string message)
    {
        if (!_config.IsLevelEnabled(level))
            return;

        var timestamp = DateTime.UtcNow;
        for (int i = 0; i < _sinks.Length; i++)
            _sinks[i].Write(level, category, message, timestamp);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Info(string category, string message) => Log(LogLevel.Info, category, message);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Warning(string category, string message) => Log(LogLevel.Warning, category, message);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Error(string category, string message) => Log(LogLevel.Error, category, message);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Build(string category, string message) => Log(LogLevel.Build, category, message);

    public void Dispose()
    {
        for (int i = 0; i < _sinks.Length; i++)
            _sinks[i].Dispose();
    }
}
