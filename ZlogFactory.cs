using LogExtension.Core;
using LogExtension.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogExtension;

/// <summary>
/// ZLogger 工厂类，提供全局日志工厂管理
/// </summary>
public static class ZLogFactory
{
    private static ILoggerFactory? _customFactory;
    private static ZLoggerConfig _defaultConfig = new();

    private static readonly Lazy<ILoggerFactory> _defaultFactory = new(
        CreateDefaultFactory,
        LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// 默认 Factory（支持线程安全的懒加载，可被 SetFactory 覆盖）
    /// </summary>
    public static ILoggerFactory Factory => _customFactory ?? _defaultFactory.Value;

    #region 配置方法

    /// <summary>
    /// 设置自定义 LoggerFactory（全局生效）
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

    #endregion

    #region 工厂创建方法

    /// <summary>
    /// 使用指定配置创建 LoggerFactory
    /// </summary>
    public static ILoggerFactory CreateFactoryWithConfig(ZLoggerConfig config)
    {
        var traceFactory = config.CreateTraceFactory();
        var infoFactory = config.CreateInfoFactory();
        return new CompositeLoggerFactory(traceFactory, infoFactory);
    }

    /// <summary>
    /// 从 IConfiguration 创建 LoggerFactory
    /// </summary>
    /// <param name="configuration">配置对象</param>
    /// <param name="configSectionName">配置节名称，默认为 "ZLogger"</param>
    /// <param name="configureLogging">可选的额外日志配置</param>
    public static ILoggerFactory CreateFactoryFromConfiguration(
        IConfiguration configuration,
        string configSectionName = "ZLogger",
        Action<ILoggingBuilder>? configureLogging = null)
    {
        var config = configuration.ParseFromConfiguration(configSectionName);
        config.AdditionalConfiguration = configureLogging;
        return CreateFactoryWithConfig(config);
    }

    /// <summary>
    /// 获取指定类型的 Logger
    /// </summary>
    public static ILogger<T> Get<T>() => Factory.CreateLogger<T>();

    #endregion

    private static ILoggerFactory CreateDefaultFactory()
    {
        return CreateFactoryWithConfig(_defaultConfig);
    }
}
