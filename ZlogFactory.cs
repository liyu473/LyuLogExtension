using Microsoft.Extensions.Logging;
using Utf8StringInterpolation;
using ZLogger;
using ZLogger.Providers;

namespace LogExtension;

public static class ZlogFactory
{
    private static ILoggerFactory? _customFactory;

    private static readonly Lazy<ILoggerFactory> _defaultFactory = new(
        CreateDefaultFactory,
        LazyThreadSafetyMode.ExecutionAndPublication
    );

    /// <summary>
    /// 可手动设置自己的 LoggerFactory（全局生效）
    /// </summary>
    public static void SetFactory(ILoggerFactory factory) => _customFactory = factory;

    /// <summary>
    /// 默认 Factory（可被覆盖），支持线程安全的懒加载
    /// </summary>
    public static ILoggerFactory Factory => _customFactory ?? _defaultFactory.Value;

    private static ILoggerFactory CreateDefaultFactory()
    {
        return LoggerFactory.Create(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(LogLevel.Trace);
            // Add to output to console // logging.AddZLoggerConsole();

            //trace 和 debug
            logging
                 .AddFilter((_, __, lvl) => lvl is LogLevel.Trace or LogLevel.Debug)
                .AddZLoggerRollingFile(options =>
                {
                    options.FilePathSelector = (timestamp, sequenceNumber) =>
                        $"logs/trace/{timestamp.ToLocalTime():yyyy-MM-dd-HH}_{sequenceNumber:000}.log";
                    options.RollingInterval = RollingInterval.Hour;
                    options.RollingSizeKB = 1024 * 2;

                    ConfigureFormatter(options);
                    ConfigureOptions(options);
                });

            // Info及以上 文件
            logging
               .AddFilter((_, __, lvl) => lvl >= LogLevel.Information)
               .AddZLoggerRollingFile(options =>
               {
                   options.FilePathSelector = (timestamp, sequenceNumber) =>
                       $"logs/{timestamp.ToLocalTime():yyyy-MM-dd-HH}_{sequenceNumber:000}.log";
                   options.RollingInterval = RollingInterval.Hour;
                   options.RollingSizeKB = 1024 * 2;

                   ConfigureFormatter(options);
                   ConfigureOptions(options);
               });
        });
    }

    private static void ConfigureFormatter(ZLoggerRollingFileOptions options)
    {
        options.UsePlainTextFormatter(formatter =>
        {
            // 前缀：时间戳 [3字符级别] [类型名:行号] + 空格
            formatter.SetPrefixFormatter(
                $"{0:local-longdate} [{1:short}] [{2}:{3}] ",
                (in MessageTemplate template, in LogInfo info) =>
                {
                    var fullTypeName = info.Category.ToString();
                    template.Format(info.Timestamp, info.LogLevel, fullTypeName, info.LineNumber);
                }
            );

            // 异常格式化
            formatter.SetExceptionFormatter(
                (writer, ex) =>
                {
                    var message = ex?.Message ?? "Unknown error";
                    var stackTrace = ex?.StackTrace ?? "No stack trace available";
                    Utf8String.Format(
                        writer,
                        $"\n异常: {message}\n堆栈: {stackTrace}"
                    );
                }
            );
        });
    }

    private static void ConfigureOptions(ZLoggerRollingFileOptions options)
    {
        // 启用调用者信息捕获
        options.IncludeScopes = true; // 启用作用域支持
        options.CaptureThreadInfo = true; // 捕获线程信息
    }

    public static ILogger<T> Get<T>() => Factory.CreateLogger<T>();
}