using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LogExtension;

/// <summary>
/// ZLogger 依赖注入扩展方法
/// </summary>
public static class ZLogServiceExtensions
{
    /// <summary>
    /// 添加 ZLogger 日志服务（使用默认配置）
    /// </summary>
    public static IServiceCollection AddZLogger(this IServiceCollection services)
    {
        return services.RegisterFactory(ZLogFactory.Factory);
    }

    /// <summary>
    /// 添加 ZLogger 日志服务（使用自定义工厂）
    /// </summary>
    public static IServiceCollection AddZLogger(
        this IServiceCollection services,
        ILoggerFactory customFactory)
    {
        ZLogFactory.SetFactory(customFactory);
        return services.RegisterFactory(customFactory);
    }

    /// <summary>
    /// 添加 ZLogger 日志服务（使用 Action 配置）
    /// </summary>
    public static IServiceCollection AddZLogger(
        this IServiceCollection services,
        Action<ZLoggerConfig> configure)
    {
        var config = new ZLoggerConfig();
        configure(config);

        var factory = ZLogFactory.CreateFactoryWithConfig(config);
        ZLogFactory.SetFactory(factory);
        return services.RegisterFactory(factory);
    }

    /// <summary>
    /// 添加 ZLogger 日志服务（仅配置额外的日志提供程序）
    /// </summary>
    public static IServiceCollection AddZLogger(
        this IServiceCollection services,
        Action<ILoggingBuilder> configureLogging)
    {
        var config = new ZLoggerConfig
        {
            AdditionalConfiguration = configureLogging
        };

        var factory = ZLogFactory.CreateFactoryWithConfig(config);
        ZLogFactory.SetFactory(factory);
        return services.RegisterFactory(factory);
    }

    /// <summary>
    /// 添加 ZLogger 日志服务（同时配置基础选项和日志提供程序）
    /// </summary>
    public static IServiceCollection AddZLogger(
        this IServiceCollection services,
        Action<ILoggingBuilder> configureLogging,
        Action<ZLoggerConfig> configure)
    {
        var config = new ZLoggerConfig();
        configure(config);
        config.AdditionalConfiguration = configureLogging;

        var factory = ZLogFactory.CreateFactoryWithConfig(config);
        ZLogFactory.SetFactory(factory);
        return services.RegisterFactory(factory);
    }

    /// <summary>
    /// 添加 ZLogger 日志服务（使用配置对象）
    /// </summary>
    public static IServiceCollection AddZLogger(
        this IServiceCollection services,
        ZLoggerConfig config)
    {
        var factory = ZLogFactory.CreateFactoryWithConfig(config);
        ZLogFactory.SetFactory(factory);
        return services.RegisterFactory(factory);
    }

    /// <summary>
    /// 添加 ZLogger 日志服务（从 IConfiguration 读取配置）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置对象</param>
    /// <param name="configSectionName">配置节名称，默认为 "ZLogger"</param>
    public static IServiceCollection AddZLogger(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSectionName = "ZLogger")
    {
        var factory = ZLogFactory.CreateFactoryFromConfiguration(configuration, configSectionName);
        ZLogFactory.SetFactory(factory);
        return services.RegisterFactory(factory);
    }

    /// <summary>
    /// 添加 ZLogger 日志服务（从 IConfiguration 读取配置并支持额外的日志提供程序）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置对象</param>
    /// <param name="configureLogging">配置额外的日志提供程序</param>
    /// <param name="configSectionName">配置节名称，默认为 "ZLogger"</param>
    public static IServiceCollection AddZLogger(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<ILoggingBuilder> configureLogging,
        string configSectionName = "ZLogger")
    {
        var factory = ZLogFactory.CreateFactoryFromConfiguration(
            configuration,
            configSectionName,
            configureLogging);
        ZLogFactory.SetFactory(factory);
        return services.RegisterFactory(factory);
    }

    /// <summary>
    /// 注册 LoggerFactory 和泛型 Logger 到服务容器
    /// </summary>
    private static IServiceCollection RegisterFactory(
        this IServiceCollection services,
        ILoggerFactory factory)
    {
        services.AddSingleton(factory);
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        return services;
    }
}
