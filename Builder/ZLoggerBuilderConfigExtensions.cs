using LogExtension.Core;
using LogExtension.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ZLogger.Providers;

namespace LogExtension.Builder;

/// <summary>
/// ZLoggerBuilder 链式配置扩展方法
/// </summary>
public static class ZLoggerBuilderConfigExtensions
{
    #region 文件输出配置

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

    #endregion

    #region 全局滚动配置

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

    #endregion

    #region 控制台配置

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

    #endregion

    #region 过滤器配置

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

    #endregion

    #region 额外配置

    /// <summary>
    /// 添加额外的日志配置
    /// </summary>
    public static ZLoggerBuilder WithAdditionalConfiguration(this ZLoggerBuilder builder, Action<ILoggingBuilder> configure)
    {
        var original = builder.Config.AdditionalConfiguration;
        builder.Config.AdditionalConfiguration = b =>
        {
            original?.Invoke(b);
            configure(b);
        };
        return builder;
    }

    /// <summary>
    /// 从 IConfiguration 加载配置
    /// </summary>
    public static ZLoggerBuilder FromConfiguration(this ZLoggerBuilder builder, IConfiguration configuration, string sectionName = "ZLogger")
    {
        var loadedConfig = configuration.ParseFromConfiguration(sectionName);

        // 合并全局配置
        builder.Config.GlobalRollingInterval = loadedConfig.GlobalRollingInterval;
        builder.Config.GlobalRollingSizeKB = loadedConfig.GlobalRollingSizeKB;

        // 合并输出配置
        foreach (var output in loadedConfig.Outputs)
        {
            builder.Config.Outputs.Add(output);
        }

        // 合并过滤器
        foreach (var filter in loadedConfig.CategoryFilters)
        {
            builder.Config.CategoryFilters[filter.Key] = filter.Value;
        }

        return builder;
    }

    #endregion
}
