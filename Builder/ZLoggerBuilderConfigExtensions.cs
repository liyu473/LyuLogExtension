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
    #region 最小日志级别

    /// <summary>
    /// 设置全局最小日志级别
    /// </summary>
    public static ZLoggerBuilder WithMinimumLevel(this ZLoggerBuilder builder, LogLevel level)
    {
        builder.Config.MinimumLevel = level;
        return builder;
    }

    /// <summary>
    /// 设置 Trace/Debug 日志的最小级别
    /// </summary>
    public static ZLoggerBuilder WithTraceMinimumLevel(this ZLoggerBuilder builder, LogLevel level)
    {
        builder.Config.TraceMinimumLevel = level;
        return builder;
    }

    #endregion

    #region 文件路径配置

    /// <summary>
    /// 设置 Trace/Debug 日志文件路径
    /// </summary>
    public static ZLoggerBuilder WithTraceLogPath(this ZLoggerBuilder builder, string path)
    {
        builder.Config.TraceLogPath = path;
        return builder;
    }

    /// <summary>
    /// 设置 Info 及以上日志文件路径
    /// </summary>
    public static ZLoggerBuilder WithInfoLogPath(this ZLoggerBuilder builder, string path)
    {
        builder.Config.InfoLogPath = path;
        return builder;
    }

    /// <summary>
    /// 同时设置两个日志路径
    /// </summary>
    public static ZLoggerBuilder WithLogPaths(this ZLoggerBuilder builder, string tracePath, string infoPath)
    {
        builder.Config.TraceLogPath = tracePath;
        builder.Config.InfoLogPath = infoPath;
        return builder;
    }

    #endregion

    #region 滚动配置

    /// <summary>
    /// 设置日志滚动间隔
    /// </summary>
    public static ZLoggerBuilder WithRollingInterval(this ZLoggerBuilder builder, RollingInterval interval)
    {
        builder.Config.RollingInterval = interval;
        return builder;
    }

    /// <summary>
    /// 设置单个日志文件最大大小（KB）
    /// </summary>
    public static ZLoggerBuilder WithRollingSizeKB(this ZLoggerBuilder builder, int sizeKB)
    {
        builder.Config.RollingSizeKB = sizeKB;
        return builder;
    }

    #endregion

    #region 控制台配置

    /// <summary>
    /// 启用控制台输出（带时间戳）
    /// </summary>
    public static ZLoggerBuilder WithConsole(this ZLoggerBuilder builder)
    {
        builder.EnableConsole = true;
        builder.EnableConsoleWithDetails = false;
        return builder;
    }

    /// <summary>
    /// 启用控制台输出（带时间戳和类名详情）
    /// </summary>
    public static ZLoggerBuilder WithConsoleDetails(this ZLoggerBuilder builder)
    {
        builder.EnableConsole = false;
        builder.EnableConsoleWithDetails = true;
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
        
        // 合并配置
        builder.Config.MinimumLevel = loadedConfig.MinimumLevel;
        builder.Config.TraceMinimumLevel = loadedConfig.TraceMinimumLevel;
        
        if (!string.IsNullOrEmpty(loadedConfig.TraceLogPath))
            builder.Config.TraceLogPath = loadedConfig.TraceLogPath;
        
        if (!string.IsNullOrEmpty(loadedConfig.InfoLogPath))
            builder.Config.InfoLogPath = loadedConfig.InfoLogPath;
        
        if (loadedConfig.RollingInterval.HasValue)
            builder.Config.RollingInterval = loadedConfig.RollingInterval;
        
        if (loadedConfig.RollingSizeKB.HasValue)
            builder.Config.RollingSizeKB = loadedConfig.RollingSizeKB;
        
        foreach (var filter in loadedConfig.CategoryFilters)
        {
            builder.Config.CategoryFilters[filter.Key] = filter.Value;
        }
        
        return builder;
    }

    #endregion
}
