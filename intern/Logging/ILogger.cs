namespace Engine.Logging;

public interface ILogger
{
    void Log(LogLevel level, string category, string message);
    void Info(string category, string message);
    void Warning(string category, string message);
    void Error(string category, string message);
    void Build(string category, string message);
}
