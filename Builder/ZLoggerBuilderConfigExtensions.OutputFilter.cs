using Microsoft.Extensions.Logging;

namespace LyuLogExtension.Builder;

/// <summary>
/// ZLoggerBuilder 输出级别过滤器配置扩展
/// </summary>
public static partial class ZLoggerBuilderConfigExtensions
{
    /// <summary>
    /// 为最后添加的文件输出设置独立过滤器
    /// </summary>
    /// <param name="builder">构建器</param>
    /// <param name="category">类别名称</param>
    /// <param name="minLevel">最小日志级别</param>
    public static ZLoggerBuilder WithOutputFilter(
        this ZLoggerBuilder builder,
        string category,
        LogLevel minLevel
    )
    {
        var lastOutput = builder.Config.Outputs.LastOrDefault();
        if (lastOutput != null)
        {
            lastOutput.CategoryFilters[category] = minLevel;
        }
        return builder;
    }

    /// <summary>
    /// 为最后添加的文件输出设置多个独立过滤器
    /// </summary>
    public static ZLoggerBuilder WithOutputFilters(
        this ZLoggerBuilder builder,
        Dictionary<string, LogLevel> filters
    )
    {
        var lastOutput = builder.Config.Outputs.LastOrDefault();
        if (lastOutput != null)
        {
            foreach (var filter in filters)
            {
                lastOutput.CategoryFilters[filter.Key] = filter.Value;
            }
        }
        return builder;
    }

    /// <summary>
    /// 设置最后添加的文件输出不使用全局过滤器
    /// </summary>
    public static ZLoggerBuilder WithoutGlobalFilters(this ZLoggerBuilder builder)
    {
        var lastOutput = builder.Config.Outputs.LastOrDefault();
        if (lastOutput != null)
        {
            lastOutput.UseGlobalFilters = false;
        }
        return builder;
    }

    /// <summary>
    /// 为控制台设置独立过滤器
    /// </summary>
    public static ZLoggerBuilder WithConsoleFilter(
        this ZLoggerBuilder builder,
        string category,
        LogLevel minLevel
    )
    {
        builder.Config.ConsoleCategoryFilters[category] = minLevel;
        return builder;
    }

    /// <summary>
    /// 为控制台设置多个独立过滤器
    /// </summary>
    public static ZLoggerBuilder WithConsoleFilters(
        this ZLoggerBuilder builder,
        Dictionary<string, LogLevel> filters
    )
    {
        foreach (var filter in filters)
        {
            builder.Config.ConsoleCategoryFilters[filter.Key] = filter.Value;
        }
        return builder;
    }

    /// <summary>
    /// 设置控制台不使用全局过滤器
    /// </summary>
    public static ZLoggerBuilder WithConsoleWithoutGlobalFilters(this ZLoggerBuilder builder)
    {
        builder.Config.ConsoleUseGlobalFilters = false;
        return builder;
    }
}
