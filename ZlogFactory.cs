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
    private static readonly ZLoggerConfig DefaultConfig = new();

    private static readonly Lazy<ILoggerFactory> LazyDefaultFactory = new(
        CreateDefaultFactory,
        LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// 获取工厂
    /// </summary>
    public static ILoggerFactory Factory => _customFactory ?? LazyDefaultFactory.Value;

    /// <summary>
    /// 获取指定类型的 Logger
    /// </summary>
    public static ILogger<T> Get<T>() => Factory.CreateLogger<T>();

    #region 内部方法

    /// <summary>
    /// 设置自定义 LoggerFactory（内部使用）
    /// </summary>
    internal static void SetFactory(ILoggerFactory factory) => _customFactory = factory;

    /// <summary>
    /// 使用指定配置创建 LoggerFactory（内部使用）
    /// </summary>
    internal static ILoggerFactory CreateFactoryWithConfig(ZLoggerConfig config)
    {
        var traceFactory = config.CreateTraceFactory();
        var infoFactory = config.CreateInfoFactory();
        return new CompositeLoggerFactory(traceFactory, infoFactory);
    }

    /// <summary>
    /// 从 IConfiguration 创建 LoggerFactory（内部使用）
    /// </summary>
    internal static ILoggerFactory CreateFactoryFromConfiguration(
        IConfiguration configuration,
        string configSectionName = "ZLogger",
        Action<ILoggingBuilder>? configureLogging = null)
    {
        var config = configuration.ParseFromConfiguration(configSectionName);
        config.AdditionalConfiguration = configureLogging;
        return CreateFactoryWithConfig(config);
    }

    private static ILoggerFactory CreateDefaultFactory()
    {
        return CreateFactoryWithConfig(DefaultConfig);
    }

    #endregion
}
