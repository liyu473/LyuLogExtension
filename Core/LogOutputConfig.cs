using Microsoft.Extensions.Logging;
using ZLogger.Providers;

namespace LyuLogExtension.Core;

/// <summary>
/// 日志输出配置（内部使用）
/// </summary>
internal class LogOutputConfig
{
    /// <summary>
    /// 日志文件路径
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    /// 最小日志级别（包含）
    /// </summary>
    public LogLevel MinLevel { get; set; } = LogLevel.Trace;

    /// <summary>
    /// 最大日志级别（包含），null 表示无上限
    /// </summary>
    public LogLevel? MaxLevel { get; set; }

    /// <summary>
    /// 日志滚动间隔（null 使用全局配置）
    /// </summary>
    public RollingInterval? RollingInterval { get; set; }

    /// <summary>
    /// 单个日志文件最大大小 KB（null 使用全局配置）
    /// </summary>
    public int? RollingSizeKB { get; set; }

    /// <summary>
    /// 此输出的独立类别过滤器（优先于全局过滤器）
    /// </summary>
    public Dictionary<string, LogLevel> CategoryFilters { get; set; } = [];

    /// <summary>
    /// 是否使用全局过滤器（默认 true）
    /// </summary>
    public bool UseGlobalFilters { get; set; } = true;
}
