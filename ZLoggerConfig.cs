using Microsoft.Extensions.Logging;
using ZLogger.Providers;

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
    /// Trace/Debug 日志文件路径（默认：logs/trace/）
    /// 可选配置，不设置则使用默认值
    /// </summary>
    public string? TraceLogPath { get; set; }

    /// <summary>
    /// Info 及以上日志文件路径（默认：logs/）
    /// 可选配置，不设置则使用默认值
    /// </summary>
    public string? InfoLogPath { get; set; }

    /// <summary>
    /// 日志滚动间隔（默认：每小时）
    /// 可选配置，不设置则使用默认值
    /// </summary>
    public RollingInterval? RollingInterval { get; set; }

    /// <summary>
    /// 单个日志文件最大大小（KB）（默认：2048KB = 2MB）
    /// 可选配置，不设置则使用默认值
    /// </summary>
    public int? RollingSizeKB { get; set; }
}
