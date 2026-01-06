namespace LyuLogExtension.Builder;

/// <summary>
/// ZLoggerBuilder 控制台配置扩展
/// </summary>
public static partial class ZLoggerBuilderConfigExtensions
{
    /// <summary>
    /// 启用控制台输出（带时间戳）
    /// </summary>
    public static ZLoggerBuilder WithConsole(this ZLoggerBuilder builder)
    {
        builder.Config.EnableConsole = true;
        builder.Config.EnableConsoleWithDetails = false;
        return builder;
    }

    /// <summary>
    /// 启用控制台输出（带时间戳和类名详情）
    /// </summary>
    public static ZLoggerBuilder WithConsoleDetails(this ZLoggerBuilder builder)
    {
        builder.Config.EnableConsole = false;
        builder.Config.EnableConsoleWithDetails = true;
        return builder;
    }
}
