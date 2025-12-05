using Microsoft.Extensions.Logging;
using ZLogger.Providers;

namespace LogExtension.Core;

/// <summary>
/// ZLogger 配置选项
/// </summary>
public class ZLoggerConfig
{
    /// <summary>
    /// 默认最低日志级别（默认：Information）
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Trace/Debug 日志的最低级别（默认：Trace）
    /// </summary>
    public LogLevel TraceMinimumLevel { get; set; } = LogLevel.Trace;

    /// <summary>
    /// Trace/Debug 日志文件路径（默认：logs/trace/）
    /// </summary>
    public string? TraceLogPath { get; set; }

    /// <summary>
    /// Info 及以上日志文件路径（默认：logs/）
    /// </summary>
    public string? InfoLogPath { get; set; }

    /// <summary>
    /// 日志滚动间隔（默认：每小时）
    /// </summary>
    public RollingInterval? RollingInterval { get; set; }

    /// <summary>
    /// 单个日志文件最大大小 KB（默认：2048KB = 2MB）
    /// </summary>
    public int? RollingSizeKB { get; set; }

    /// <summary>
    /// 日志级别过滤规则
    /// Key: 类别名称（如 "System.Net.Http"）
    /// Value: 最低日志级别
    /// </summary>
    public Dictionary<string, LogLevel> CategoryFilters { get; set; } = new();

    /// <summary>
    /// 额外的日志配置回调（如添加控制台、Debug 输出等）
    /// </summary>
    public Action<ILoggingBuilder>? AdditionalConfiguration { get; set; }
}
