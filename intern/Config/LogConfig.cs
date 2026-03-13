using Engine.Logging;

namespace Engine.Config;

public sealed class LogConfig
{
    public bool InfoEnabled { get; set; } = true;
    public bool WarningEnabled { get; set; } = true;
    public bool ErrorEnabled { get; set; } = true;
    public bool BuildEnabled { get; set; } = true;
    public bool ExposeLogsInBuild { get; set; } = false;
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10MB
    public int MaxRotatedFiles { get; set; } = 3;

    public bool IsLevelEnabled(LogLevel level) => level switch
    {
        LogLevel.Info => InfoEnabled,
        LogLevel.Warning => WarningEnabled,
        LogLevel.Error => ErrorEnabled,
        LogLevel.Build => BuildEnabled,
        _ => false
    };
}
