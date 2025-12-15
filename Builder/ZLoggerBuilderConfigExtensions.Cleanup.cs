namespace LogExtension.Builder;

/// <summary>
/// ZLoggerBuilder 日志清理配置扩展
/// </summary>
public static partial class ZLoggerBuilderConfigExtensions
{
    /// <summary>
    /// 设置日志文件保留天数
    /// </summary>
    /// <param name="builder">构建器</param>
    /// <param name="days">保留天数（0 表示不清理）</param>
    public static ZLoggerBuilder WithRetentionDays(this ZLoggerBuilder builder, int days)
    {
        builder.Config.RetentionDays = days;
        return builder;
    }

    /// <summary>
    /// 禁用后台定时清理（仅启动时清理一次）
    /// </summary>
    public static ZLoggerBuilder DisableBackgroundCleanup(this ZLoggerBuilder builder)
    {
        builder.Config.EnableBackgroundCleanup = false;
        return builder;
    }

    /// <summary>
    /// 设置后台清理间隔
    /// </summary>
    /// <param name="builder">构建器</param>
    /// <param name="interval">清理间隔</param>
    public static ZLoggerBuilder WithCleanupInterval(this ZLoggerBuilder builder, TimeSpan interval)
    {
        builder.Config.CleanupInterval = interval;
        return builder;
    }
}
