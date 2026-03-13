namespace Engine.Logging;

public abstract class LogSink : IDisposable
{
    public abstract void Write(LogLevel level, string category, string message, DateTime timestamp);
    public virtual void Flush() { }
    public virtual void Dispose() { }
}
