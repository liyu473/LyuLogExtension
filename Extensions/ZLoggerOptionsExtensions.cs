using LyuLogExtension.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Utf8StringInterpolation;
using ZLogger;
using ZLogger.Providers;

namespace LyuLogExtension.Extensions;

/// <summary>
/// ZLogger 内部配置扩展方法
/// </summary>
internal static class ZLoggerOptionsExtensions
{
    /// <summary>
    /// 为指定输出配置创建 LoggerFactory
    /// </summary>
    internal static ILoggerFactory CreateFactoryForOutput(this LogOutputConfig output, ZLoggerConfig globalConfig)
    {
        return LoggerFactory.Create(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(output.MinLevel);

            logging.AddZLoggerRollingFile(options =>
            {
                options.FilePathSelector = (timestamp, sequenceNumber) =>
                    $"{output.Path}{timestamp.ToLocalTime():yyyy-MM-dd-HH}_{sequenceNumber:000}.log";
                options.RollingInterval = output.RollingInterval ?? globalConfig.GlobalRollingInterval;
                options.RollingSizeKB = output.RollingSizeKB ?? globalConfig.GlobalRollingSizeKB;

                options.ConfigureFormatter();
                options.ConfigureOptions();
            });

            // 应用级别过滤（min 和 max）
            if (output.MaxLevel.HasValue)
            {
                logging.AddFilter((_, level) => level >= output.MinLevel && level <= output.MaxLevel.Value);
            }
            else
            {
                logging.AddFilter((_, level) => level >= output.MinLevel);
            }

            // 应用全局类别过滤器
            if (output.UseGlobalFilters)
            {
                foreach (var filter in globalConfig.CategoryFilters)
                {
                    logging.AddFilter(filter.Key, filter.Value);
                }
            }

            // 应用此输出的独立过滤器（会覆盖全局）
            foreach (var filter in output.CategoryFilters)
            {
                logging.AddFilter(filter.Key, filter.Value);
            }
        });
    }

    /// <summary>
    /// 创建控制台 LoggerFactory
    /// </summary>
    internal static ILoggerFactory CreateConsoleFactory(this ZLoggerConfig config)
    {
        return LoggerFactory.Create(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(LogLevel.Trace);

            if (config.EnableConsoleWithDetails)
                logging.AddZLoggerConsoleWithDetails();
            else
                logging.AddZLoggerConsoleWithTimestamp();

            // 应用额外配置
            config.AdditionalConfiguration?.Invoke(logging);

            // 应用全局类别过滤器
            if (config.ConsoleUseGlobalFilters)
            {
                foreach (var filter in config.CategoryFilters)
                {
                    logging.AddFilter(filter.Key, filter.Value);
                }
            }

            // 应用控制台独立过滤器（会覆盖全局）
            foreach (var filter in config.ConsoleCategoryFilters)
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
            formatter.SetPrefixFormatter(
                $"{0:local-longdate} [{1:short}] [{2}:{3}] ",
                (in MessageTemplate template, in LogInfo info) =>
                    template.Format(info.Timestamp, info.LogLevel, info.Category, info.LineNumber));

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

        // 解析全局滚动配置
        if (Enum.TryParse<RollingInterval>(section["GlobalRollingInterval"], out var interval))
            config.GlobalRollingInterval = interval;

        if (int.TryParse(section["GlobalRollingSizeKB"], out var sizeKB))
            config.GlobalRollingSizeKB = sizeKB;

        // 解析输出配置
        var outputsSection = section.GetSection("Outputs");
        if (outputsSection.Exists())
        {
            foreach (var outputSection in outputsSection.GetChildren())
            {
                var output = new LogOutputConfig
                {
                    Path = outputSection["Path"] ?? "logs/"
                };

                if (Enum.TryParse<LogLevel>(outputSection["MinLevel"], out var minLevel))
                    output.MinLevel = minLevel;

                if (Enum.TryParse<LogLevel>(outputSection["MaxLevel"], out var maxLevel))
                    output.MaxLevel = maxLevel;

                if (Enum.TryParse<RollingInterval>(outputSection["RollingInterval"], out var outputInterval))
                    output.RollingInterval = outputInterval;

                if (int.TryParse(outputSection["RollingSizeKB"], out var outputSizeKB))
                    output.RollingSizeKB = outputSizeKB;

                config.Outputs.Add(output);
            }
        }

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
