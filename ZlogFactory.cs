using LyuLogExtension.Extensions;
using LyuLogExtension.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LyuLogExtension;

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
    /// 默认 Factory（支持线程安全的懒加载，可被内部 SetFactory 覆盖）
    /// </summary>
    public static ILoggerFactory Factory => _customFactory ?? LazyDefaultFactory.Value;

    /// <summary>
    /// 获取指定类型的 Logger
    /// </summary>
    public static ILogger<T> Get<T>() => Factory.CreateLogger<T>();

    /// <summary>
    /// 配置初始化全局 LoggerFactory（静态方式）
    /// </summary>
    public static void Configure(Action<Builder.ZLoggerBuilder> configure)
    {
        var builder = new Builder.ZLoggerBuilder();
        configure(builder);
        _customFactory = builder.Build();
    }

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
        var factories = new List<ILoggerFactory>();

        foreach (var output in config.Outputs)
        {
            factories.Add(output.CreateFactoryForOutput(config));
        }

        // 如果没有配置任何输出，添加默认输出
        if (factories.Count == 0)
        {
            var defaultOutput = new LogOutputConfig { Path = "logs/" };
            factories.Add(defaultOutput.CreateFactoryForOutput(config));
        }

        // 添加控制台工厂
        if (config.EnableConsole || config.EnableConsoleWithDetails)
        {
            factories.Add(config.CreateConsoleFactory());
        }

        return new CompositeLoggerFactory([.. factories]);
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
