using Microsoft.Extensions.Logging;

namespace LyuLogExtension.Builder;

/// <summary>
/// ZLoggerBuilder 过滤器配置扩展
/// </summary>
public static partial class ZLoggerBuilderConfigExtensions
{
    /// <summary>
    /// 添加类别过滤器
    /// </summary>
    public static ZLoggerBuilder WithFilter(this ZLoggerBuilder builder, string category, LogLevel minLevel)
    {
        builder.Config.CategoryFilters[category] = minLevel;
        return builder;
    }

    /// <summary>
    /// 添加多个类别过滤器
    /// </summary>
    public static ZLoggerBuilder WithFilters(this ZLoggerBuilder builder, Dictionary<string, LogLevel> filters)
    {
        foreach (var filter in filters)
        {
            builder.Config.CategoryFilters[filter.Key] = filter.Value;
        }
        return builder;
    }

    /// <summary>
    /// 过滤 Microsoft 命名空间日志
    /// </summary>
    public static ZLoggerBuilder FilterMicrosoft(this ZLoggerBuilder builder, LogLevel minLevel = LogLevel.Warning)
    {
        builder.Config.CategoryFilters["Microsoft"] = minLevel;
        return builder;
    }

    /// <summary>
    /// 过滤 System 命名空间日志
    /// </summary>
    public static ZLoggerBuilder FilterSystem(this ZLoggerBuilder builder, LogLevel minLevel = LogLevel.Warning)
    {
        builder.Config.CategoryFilters["System"] = minLevel;
        return builder;
    }
}
