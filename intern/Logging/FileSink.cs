using Engine.Config;

namespace Engine.Logging;

public sealed class FileSink : LogSink
{
    private readonly Dictionary<LogLevel, StreamWriter> _writers = new();
    private readonly string _logsDirectory;
    private readonly long _maxFileSize;
    private readonly int _maxRotatedFiles;

    public FileSink(LogConfig config)
    {
        _logsDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        _maxFileSize = config.MaxFileSizeBytes;
        _maxRotatedFiles = config.MaxRotatedFiles;

        Directory.CreateDirectory(_logsDirectory);

        foreach (LogLevel level in Enum.GetValues<LogLevel>())
        {
            if (config.IsLevelEnabled(level))
            {
                string path = GetLogPath(level);
                var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
                _writers[level] = new StreamWriter(stream) { AutoFlush = true };
            }
        }
    }

    public override void Write(LogLevel level, string category, string message, DateTime timestamp)
    {
        if (!_writers.TryGetValue(level, out var writer))
            return;

        var line = $"[{timestamp:yyyy-MM-ddTHH:mm:ss.fff}] [{level}] [{category}] {message}";
        writer.WriteLine(line);

        RotateIfNeeded(level, writer);
    }

    private void RotateIfNeeded(LogLevel level, StreamWriter writer)
    {
        if (writer.BaseStream.Length <= _maxFileSize)
            return;

        writer.Flush();
        writer.Dispose();

        string path = GetLogPath(level);

        for (int i = _maxRotatedFiles - 1; i >= 1; i--)
        {
            string src = $"{path}.{i}";
            string dst = $"{path}.{i + 1}";
            if (File.Exists(src))
            {
                if (File.Exists(dst)) File.Delete(dst);
                File.Move(src, dst);
            }
        }

        if (File.Exists(path))
        {
            string first = $"{path}.1";
            if (File.Exists(first)) File.Delete(first);
            File.Move(path, first);
        }

        var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        _writers[level] = new StreamWriter(stream) { AutoFlush = true };
    }

    private string GetLogPath(LogLevel level)
    {
        return Path.Combine(_logsDirectory, $"{level.ToString().ToLowerInvariant()}.log");
    }

    public override void Flush()
    {
        foreach (var writer in _writers.Values)
            writer.Flush();
    }

    public override void Dispose()
    {
        foreach (var writer in _writers.Values)
            writer.Dispose();
        _writers.Clear();
    }
}
