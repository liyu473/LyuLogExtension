using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Utf8StringInterpolation;
using ZLogger;
using ZLogger.Providers;

namespace LogExtension;

/// <summary>
/// ZLogger 内部配置扩展方法
/// </summary>
internal static class ZLoggerOptionsExtensions
{
    /// <summary>
    /// 创建 Trace/Debug 日志工厂
    /// </summary>
    internal static ILoggerFactory CreateTraceFactory(this ZLoggerConfig config)
    {
        return LoggerFactory.Create(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(config.TraceMinimumLevel);

            logging.AddZLoggerRollingFile(options =>
            {
                var tracePath = config.TraceLogPath ?? "logs/trace/";
                options.FilePathSelector = (timestamp, sequenceNumber) =>
                    $"{tracePath}{timestamp.ToLocalTime():yyyy-MM-dd-HH}_{sequenceNumber:000}.log";
                options.RollingInterval = config.RollingInterval ?? RollingInterval.Hour;
                options.RollingSizeKB = config.RollingSizeKB ?? 2048;

                options.ConfigureFormatter();
                options.ConfigureOptions();
            });

            // 只记录 Trace 和 Debug
            logging.AddFilter((_, level) => level < LogLevel.Information);
        });
    }

    /// <summary>
    /// 创建 Info 及以上日志工厂
    /// </summary>
    internal static ILoggerFactory CreateInfoFactory(this ZLoggerConfig config)
    {
        return LoggerFactory.Create(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(config.MinimumLevel);

            logging.AddZLoggerRollingFile(options =>
            {
                var infoPath = config.InfoLogPath ?? "logs/";
                options.FilePathSelector = (timestamp, sequenceNumber) =>
                    $"{infoPath}{timestamp.ToLocalTime():yyyy-MM-dd-HH}_{sequenceNumber:000}.log";
                options.RollingInterval = config.RollingInterval ?? RollingInterval.Hour;
                options.RollingSizeKB = config.RollingSizeKB ?? 2048;

                options.ConfigureFormatter();
                options.ConfigureOptions();
            });

            // 应用额外配置（如控制台输出等）
            config.AdditionalConfiguration?.Invoke(logging);

            // 应用类别过滤器
            foreach (var filter in config.CategoryFilters)
            {
                logging.AddFilter(filter.Key, filter.Value);
            }
        });
    }

    /// <summary>
    /// 配置日志格式化器
    /// </summary>
    internal static void ConfigureFormatter(this ZLoggerRollingFileOptions options)
    {
        options.UsePlainTextFormatter(formatter =>
        {
            // 前缀：时间戳 [3字符级别] [类型名:行号]
            formatter.SetPrefixFormatter(
                $"{0:local-longdate} [{1:short}] [{2}:{3}] ",
                (in MessageTemplate template, in LogInfo info) =>
                    template.Format(info.Timestamp, info.LogLevel, info.Category, info.LineNumber));

            // 异常格式化
            formatter.SetExceptionFormatter((writer, ex) =>
            {
                var message = ex?.Message ?? "Unknown error";
                var stackTrace = ex?.StackTrace ?? "No stack trace available";
                Utf8String.Format(writer, $"\n异常: {message}\n堆栈: {stackTrace}");
            });
        });
    }

    /// <summary>
    /// 配置日志选项
    /// </summary>
    internal static void ConfigureOptions(this ZLoggerRollingFileOptions options)
    {
        options.IncludeScopes = true;
        options.CaptureThreadInfo = true;
        // 注意：标准的 ILogger.LogInformation 无法自动获取行号
        // 建议使用 ZLogger 的专用方法：logger.ZLogInformation($"消息")
    }

    /// <summary>
    /// 从 IConfiguration 解析配置
    /// </summary>
    internal static ZLoggerConfig ParseFromConfiguration(
        this IConfiguration configuration,
        string sectionName)
    {
        var config = new ZLoggerConfig();
        var section = configuration.GetSection(sectionName);

        if (!section.Exists())
            return config;

        // 解析日志级别
        if (Enum.TryParse<LogLevel>(section["MinimumLevel"], out var minLevel))
            config.MinimumLevel = minLevel;

        if (Enum.TryParse<LogLevel>(section["TraceMinimumLevel"], out var traceLevel))
            config.TraceMinimumLevel = traceLevel;

        // 解析文件路径
        var traceLogPath = section["TraceLogPath"];
        if (!string.IsNullOrWhiteSpace(traceLogPath))
            config.TraceLogPath = traceLogPath;

        var infoLogPath = section["InfoLogPath"];
        if (!string.IsNullOrWhiteSpace(infoLogPath))
            config.InfoLogPath = infoLogPath;

        // 解析滚动配置
        if (Enum.TryParse<RollingInterval>(section["RollingInterval"], out var interval))
            config.RollingInterval = interval;

        if (int.TryParse(section["RollingSizeKB"], out var sizeKB))
            config.RollingSizeKB = sizeKB;

        // 解析类别过滤器
        var logLevelSection = section.GetSection("LogLevel");
        if (logLevelSection.Exists())
        {
            foreach (var item in logLevelSection.GetChildren())
            {
                if (Enum.TryParse<LogLevel>(item.Value, out var level))
                    config.CategoryFilters[item.Key] = level;
            }
        }

        return config;
    }
}
