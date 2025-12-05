using Microsoft.Extensions.Logging;
using ZLogger.Providers;

namespace LogExtension.Core;

/// <summary>
/// ZLogger 配置选项（内部使用）
/// </summary>
internal class ZLoggerConfig
{
    /// <summary>
    /// 日志输出配置列表
    /// </summary>
    public List<LogOutputConfig> Outputs { get; set; } = [];

    /// <summary>
    /// 全局日志滚动间隔（默认：每小时）
    /// </summary>
    public RollingInterval GlobalRollingInterval { get; set; } = RollingInterval.Hour;

    /// <summary>
    /// 全局单个日志文件最大大小 KB（默认：2048KB = 2MB）
    /// </summary>
    public int GlobalRollingSizeKB { get; set; } = 2048;

    /// <summary>
    /// 日志级别过滤规则
    /// Key: 类别名称（如 "System.Net.Http"）
    /// Value: 最低日志级别
    /// </summary>
    public Dictionary<string, LogLevel> CategoryFilters { get; set; } = [];

    /// <summary>
    /// 额外的日志配置回调（如添加控制台、Debug 输出等）
    /// </summary>
    public Action<ILoggingBuilder>? AdditionalConfiguration { get; set; }

    /// <summary>
    /// 是否启用控制台输出
    /// </summary>
    public bool EnableConsole { get; set; }

    /// <summary>
    /// 是否启用控制台详情输出（带类名）
    /// </summary>
    public bool EnableConsoleWithDetails { get; set; }

    /// <summary>
    /// 控制台独立过滤器（优先于全局过滤器）
    /// </summary>
    public Dictionary<string, LogLevel> ConsoleCategoryFilters { get; set; } = [];

    /// <summary>
    /// 控制台是否使用全局过滤器（默认 true）
    /// </summary>
    public bool ConsoleUseGlobalFilters { get; set; } = true;
}
