namespace Engine.Logging;

public sealed class ConsoleSink : LogSink
{
    public override void Write(LogLevel level, string category, string message, DateTime timestamp)
    {
        var prevColor = Console.ForegroundColor;
        Console.ForegroundColor = level switch
        {
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Build => ConsoleColor.Cyan,
            _ => ConsoleColor.Gray
        };

        Console.WriteLine($"[{timestamp:HH:mm:ss.fff}] [{level}] [{category}] {message}");
        Console.ForegroundColor = prevColor;
    }
}
