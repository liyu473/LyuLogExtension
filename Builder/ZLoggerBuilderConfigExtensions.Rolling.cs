using ZLogger.Providers;

namespace LogExtension.Builder;

/// <summary>
/// ZLoggerBuilder 滚动配置扩展
/// </summary>
public static partial class ZLoggerBuilderConfigExtensions
{
    /// <summary>
    /// 设置全局日志滚动间隔
    /// </summary>
    public static ZLoggerBuilder WithRollingInterval(this ZLoggerBuilder builder, RollingInterval interval)
    {
        builder.Config.GlobalRollingInterval = interval;
        return builder;
    }

    /// <summary>
    /// 设置全局单个日志文件最大大小（KB）
    /// </summary>
    public static ZLoggerBuilder WithRollingSizeKB(this ZLoggerBuilder builder, int sizeKB)
    {
        builder.Config.GlobalRollingSizeKB = sizeKB;
        return builder;
    }
}
