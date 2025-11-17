using Microsoft.Extensions.Logging;

namespace LogExtension;

/// <summary>
/// ZLogger 配置选项
/// </summary>
public class ZLoggerConfig
{
    /// <summary>
    /// 日志级别过滤规则，Key: 类别名称（如 "System.Net.Http"），Value: 最低日志级别
    /// </summary>
    public Dictionary<string, LogLevel> CategoryFilters { get; set; } = new();

    /// <summary>
    /// 默认最低日志级别
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Trace/Debug 日志的最低级别
    /// </summary>
    public LogLevel TraceMinimumLevel { get; set; } = LogLevel.Trace;

    /// <summary>
    /// 是否启用从 appsettings.json 读取配置
    /// </summary>
    public bool UseConfigurationFile { get; set; } = true;

    /// <summary>
    /// appsettings.json 文件路径（默认为当前目录）
    /// </summary>
    public string? ConfigurationFilePath { get; set; }
}
