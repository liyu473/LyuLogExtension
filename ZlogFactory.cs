using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
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

            if (config.UseConfigurationFile)
            {
                ApplyConfigurationFile(logging, config.ConfigurationFilePath);
            }
            logging.AddZLoggerRollingFile(options =>
            {
                options.FilePathSelector = (timestamp, sequenceNumber) =>
                    $"logs/trace/{timestamp.ToLocalTime():yyyy-MM-dd-HH}_{sequenceNumber:000}.log";
                options.RollingInterval = RollingInterval.Hour;
                options.RollingSizeKB = 1024 * 2;

                ConfigureFormatter(options);
                ConfigureOptions(options);
            });

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

            // 从配置文件加载配置（如果启用）
            if (config.UseConfigurationFile)
            {
                ApplyConfigurationFile(logging, config.ConfigurationFilePath);
            }
            logging.AddZLoggerRollingFile(options =>
            {
                options.FilePathSelector = (timestamp, sequenceNumber) =>
                    $"logs/{timestamp.ToLocalTime():yyyy-MM-dd-HH}_{sequenceNumber:000}.log";
                options.RollingInterval = RollingInterval.Hour;
                options.RollingSizeKB = 1024 * 2;

                ConfigureFormatter(options);
                ConfigureOptions(options);
            });

            ApplyFilters(logging, config.CategoryFilters);
        });

        // 返回组合工厂
        return new CompositeLoggerFactory(traceFactory, infoFactory);
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

    /// <summary>
    /// 应用配置文件（appsettings.json）
    /// </summary>
    private static void ApplyConfigurationFile(ILoggingBuilder logging, string? configFilePath)
    {
        try
        {
            var basePath = configFilePath ?? Directory.GetCurrentDirectory();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var loggingSection = configuration.GetSection("ZLogger");
            if (loggingSection.Exists())
            {
                logging.AddConfiguration(loggingSection);
            }
        }
        catch
        {
            // 如果配置文件不存在或读取失败，静默失败
        }
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