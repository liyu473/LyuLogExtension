using LyuLogExtension.Core;
using Microsoft.Extensions.Logging;
using ZLogger.Providers;

namespace LyuLogExtension.Builder;

/// <summary>
/// ZLoggerBuilder 文件输出配置扩展
/// </summary>
public static partial class ZLoggerBuilderConfigExtensions
{
    /// <summary>
    /// 添加文件输出
    /// </summary>
    /// <param name="builder">构建器</param>
    /// <param name="path">日志文件路径</param>
    /// <param name="minLevel">最小日志级别（包含）</param>
    /// <param name="maxLevel">最大日志级别（包含），null 表示无上限</param>
    public static ZLoggerBuilder AddFileOutput(
        this ZLoggerBuilder builder,
        string path,
        LogLevel minLevel = LogLevel.Trace,
        LogLevel? maxLevel = null)
    {
        builder.Config.Outputs.Add(new LogOutputConfig
        {
            Path = path,
            MinLevel = minLevel,
            MaxLevel = maxLevel
        });
        return builder;
    }

    /// <summary>
    /// 添加文件输出（带滚动配置）
    /// </summary>
    public static ZLoggerBuilder AddFileOutput(
        this ZLoggerBuilder builder,
        string path,
        LogLevel minLevel,
        LogLevel? maxLevel,
        RollingInterval rollingInterval,
        int rollingSizeKB)
    {
        builder.Config.Outputs.Add(new LogOutputConfig
        {
            Path = path,
            MinLevel = minLevel,
            MaxLevel = maxLevel,
            RollingInterval = rollingInterval,
            RollingSizeKB = rollingSizeKB
        });
        return builder;
    }

    /// <summary>
    /// 添加 Trace 文件输出（Trace + Debug）
    /// </summary>
    public static ZLoggerBuilder AddTraceOutput(this ZLoggerBuilder builder, string path = "logs/trace/")
    {
        return builder.AddFileOutput(path, LogLevel.Trace, LogLevel.Debug);
    }

    /// <summary>
    /// 添加 Info 文件输出（Information 及以上）
    /// </summary>
    public static ZLoggerBuilder AddInfoOutput(this ZLoggerBuilder builder, string path = "logs/")
    {
        return builder.AddFileOutput(path, LogLevel.Information);
    }

    /// <summary>
    /// 添加 Error 文件输出（Error 及以上）
    /// </summary>
    public static ZLoggerBuilder AddErrorOutput(this ZLoggerBuilder builder, string path = "logs/error/")
    {
        return builder.AddFileOutput(path, LogLevel.Error);
    }
}
