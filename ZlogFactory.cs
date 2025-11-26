using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Utf8StringInterpolation;
using ZLogger;
using ZLogger.Providers;

namespace LogExtension;

public static class ZlogFactory
{
    private static ILoggerFactory? _customFactory;
    private static ZLoggerConfig _defaultConfig = new();

    private static readonly Lazy<ILoggerFactory> _defaultFactory = new(
        CreateDefaultFactory,
        LazyThreadSafetyMode.ExecutionAndPublication
    );

    /// <summary>
    /// 可手动设置自己的 LoggerFactory（全局生效）
    /// </summary>
    public static void SetFactory(ILoggerFactory factory) => _customFactory = factory;

    /// <summary>
    /// 设置默认配置（全局生效）
    /// </summary>
    public static void SetDefaultConfig(ZLoggerConfig config) => _defaultConfig = config;

    /// <summary>
    /// 设置默认配置（通过 Action 配置）
    /// </summary>
    public static void ConfigureDefaults(Action<ZLoggerConfig> configure)
    {
        _defaultConfig = new ZLoggerConfig();
        configure(_defaultConfig);
    }

    /// <summary>
    /// 默认 Factory（可被覆盖），支持线程安全的懒加载
    /// </summary>
    public static ILoggerFactory Factory => _customFactory ?? _defaultFactory.Value;

    private static ILoggerFactory CreateDefaultFactory()
    {
        return CreateFactoryWithConfig(_defaultConfig);
    }

    /// <summary>
    /// 使用指定配置创建 LoggerFactory
    /// </summary>
    public static ILoggerFactory CreateFactoryWithConfig(ZLoggerConfig config)
    {
        // 创建 Trace/Debug 日志工厂
        var traceFactory = LoggerFactory.Create(logging =>
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

                ConfigureFormatter(options);
                ConfigureOptions(options);
            });

            // 应用额外配置（如控制台输出等）
            config.AdditionalConfiguration?.Invoke(logging);

            // 应用自定义过滤器
            ApplyFilters(logging, config.CategoryFilters);

            // 只记录 Trace 和 Debug
            logging.AddFilter((category, level) => level < LogLevel.Information);
        });

        // 创建 Info 及以上日志工厂
        var infoFactory = LoggerFactory.Create(logging =>
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

                ConfigureFormatter(options);
                ConfigureOptions(options);
            });

            // 应用额外配置（如控制台输出等）
            config.AdditionalConfiguration?.Invoke(logging);

            ApplyFilters(logging, config.CategoryFilters);
        });

        // 返回组合工厂
        return new CompositeLoggerFactory(traceFactory, infoFactory);
    }

    /// <summary>
    /// 从 IConfiguration 创建 LoggerFactory（推荐方式，类似 Serilog）
    /// </summary>
    /// <param name="configuration">配置对象</param>
    /// <param name="configSectionName">配置节名称，默认为 "ZLogger"</param>
    /// <param name="configureLogging">可选的额外日志配置（如控制台输出等）</param>
    public static ILoggerFactory CreateFactoryFromConfiguration(
        IConfiguration configuration,
        string configSectionName = "ZLogger",
        Action<ILoggingBuilder>? configureLogging = null)
    {
        var config = new ZLoggerConfig();
        var configSection = configuration.GetSection(configSectionName);

        // 从配置中读取日志级别
        if (configSection.Exists())
        {
            var minLevel = configSection["MinimumLevel"];
            if (Enum.TryParse<LogLevel>(minLevel, out var parsedMinLevel))
            {
                config.MinimumLevel = parsedMinLevel;
            }

            var traceLevel = configSection["TraceMinimumLevel"];
            if (Enum.TryParse<LogLevel>(traceLevel, out var parsedTraceLevel))
            {
                config.TraceMinimumLevel = parsedTraceLevel;
            }

            // 读取文件路径配置
            var traceLogPath = configSection["TraceLogPath"];
            if (!string.IsNullOrWhiteSpace(traceLogPath))
            {
                config.TraceLogPath = traceLogPath;
            }

            var infoLogPath = configSection["InfoLogPath"];
            if (!string.IsNullOrWhiteSpace(infoLogPath))
            {
                config.InfoLogPath = infoLogPath;
            }

            // 读取滚动间隔配置
            var rollingInterval = configSection["RollingInterval"];
            if (Enum.TryParse<RollingInterval>(rollingInterval, out var parsedInterval))
            {
                config.RollingInterval = parsedInterval;
            }

            // 读取文件大小配置
            var rollingSizeKB = configSection["RollingSizeKB"];
            if (int.TryParse(rollingSizeKB, out var parsedSize))
            {
                config.RollingSizeKB = parsedSize;
            }

            // 读取类别过滤器
            var logLevelSection = configSection.GetSection("LogLevel");
            if (logLevelSection.Exists())
            {
                foreach (var item in logLevelSection.GetChildren())
                {
                    if (Enum.TryParse<LogLevel>(item.Value, out var level))
                    {
                        config.CategoryFilters[item.Key] = level;
                    }
                }
            }
        }

        // 设置额外的日志配置
        config.AdditionalConfiguration = configureLogging;

        // 使用读取到的配置创建工厂
        return CreateFactoryWithConfig(config);
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

        // 注意：标准的 ILogger.LogInformation 无法自动获取行号
        // 建议使用 ZLogger 的专用方法：logger.ZLogInformation($"消息")
        // 如果必须使用 LogInformation，行号将显示为 0
    }

    /// <summary>
    /// 应用自定义过滤器
    /// </summary>
    private static void ApplyFilters(ILoggingBuilder logging, Dictionary<string, LogLevel> filters)
    {
        foreach (var filter in filters)
        {
            logging.AddFilter(filter.Key, filter.Value);
        }
    }

    public static ILogger<T> Get<T>() => Factory.CreateLogger<T>();
}

// 组合多个 LoggerFactory
internal class CompositeLoggerFactory(params ILoggerFactory[] factories) : ILoggerFactory
{
    private readonly ILoggerFactory[] _factories = factories;

    public void AddProvider(ILoggerProvider provider)
    {
        foreach (var factory in _factories) factory.AddProvider(provider);
    }

    public ILogger CreateLogger(string categoryName)
    {
        var loggers = _factories.Select(f => f.CreateLogger(categoryName)).ToArray();
        return new CompositeLogger(loggers);
    }

    public void Dispose()
    {
        foreach (var factory in _factories) factory.Dispose();
    }
}

// 组合多个 Logger，同时写入
internal class CompositeLogger(params ILogger[] loggers) : ILogger
{
    private readonly ILogger[] _loggers = loggers;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        var scopes = new IDisposable?[_loggers.Length];
        for (int i = 0; i < _loggers.Length; i++)
        {
            scopes[i] = _loggers[i].BeginScope(state);
        }
        return new CompositeDisposable(scopes);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        for (int i = 0; i < _loggers.Length; i++)
        {
            if (_loggers[i].IsEnabled(logLevel)) return true;
        }
        return false;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        for (int i = 0; i < _loggers.Length; i++)
        {
            _loggers[i].Log(logLevel, eventId, state, exception, formatter);
        }
    }

    private class CompositeDisposable : IDisposable
    {
        private readonly IDisposable?[] _disposables;

        public CompositeDisposable(IDisposable?[] disposables) => _disposables = disposables;

        public void Dispose()
        {
            foreach (var disposable in _disposables)
                disposable?.Dispose();
        }
    }
}