using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LogExtension;

/// <summary>
/// ZLogger 依赖注入扩展方法
/// </summary>
public static class ZlogServiceExtensions
{
    /// <summary>
    /// 添加 ZLogger 日志服务，使用默认配置（与 ZlogFactory 一致）
    /// </summary>
    public static IServiceCollection AddZLogger(this IServiceCollection services)
    {
        // 直接使用 ZlogFactory 的默认工厂
        services.AddSingleton(ZlogFactory.Factory);

        // 添加 ILogger<T> 泛型支持
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        return services;
    }

    /// <summary>
    /// 添加 ZLogger 日志服务，使用自定义工厂
    /// </summary>
    public static IServiceCollection AddZLogger(
        this IServiceCollection services,
        ILoggerFactory customFactory
    )
    {
        ZlogFactory.SetFactory(customFactory);
        services.AddSingleton(customFactory);
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        return services;
    }

    /// <summary>
    /// 添加 ZLogger 日志服务，使用 Action 配置
    /// </summary>
    public static IServiceCollection AddZLogger(
        this IServiceCollection services,
        Action<ZLoggerConfig> configure
    )
    {
        var config = new ZLoggerConfig();
        configure(config);

        var factory = ZlogFactory.CreateFactoryWithConfig(config);
        ZlogFactory.SetFactory(factory);
        services.AddSingleton(factory);
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        return services;
    }

    /// <summary>
    /// 添加 ZLogger 日志服务，同时配置基础选项和日志提供程序
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置 ZLoggerConfig</param>
    /// <param name="configureLogging">配置额外的日志提供程序（如控制台、Debug等）</param>
    public static IServiceCollection AddZLogger(
        this IServiceCollection services,
        Action<ILoggingBuilder> configureLogging,
        Action<ZLoggerConfig> configure
    )
    {
        var config = new ZLoggerConfig();
        configure(config);
        config.AdditionalConfiguration = configureLogging;

        var factory = ZlogFactory.CreateFactoryWithConfig(config);
        ZlogFactory.SetFactory(factory);
        services.AddSingleton(factory);
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        return services;
    }

    /// <summary>
    /// 添加 ZLogger 日志服务，使用配置对象
    /// </summary>
    public static IServiceCollection AddZLogger(
        this IServiceCollection services,
        ZLoggerConfig config
    )
    {
        var factory = ZlogFactory.CreateFactoryWithConfig(config);
        ZlogFactory.SetFactory(factory);
        services.AddSingleton(factory);
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        return services;
    }

    /// <summary>
    /// 添加 ZLogger 日志服务，从 IConfiguration 读取配置
    /// 默认从 "ZLogger" 配置节读取
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置对象</param>
    /// <param name="configSectionName">配置节名称，默认为 "ZLogger"</param>
    public static IServiceCollection AddZLogger(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSectionName = "ZLogger"
    )
    {
        var factory = ZlogFactory.CreateFactoryFromConfiguration(configuration, configSectionName);
        ZlogFactory.SetFactory(factory);
        services.AddSingleton(factory);
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        return services;
    }

    /// <summary>
    /// 添加 ZLogger 日志服务，从 IConfiguration 读取配置并支持额外的日志提供程序
    /// 默认从 "ZLogger" 配置节读取
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置对象</param>
    /// <param name="configureLogging">配置额外的日志提供程序（如控制台、Debug等）</param>
    /// <param name="configSectionName">配置节名称，默认为 "ZLogger"</param>
    public static IServiceCollection AddZLogger(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<ILoggingBuilder> configureLogging,
        string configSectionName = "ZLogger"
    )
    {
        var factory = ZlogFactory.CreateFactoryFromConfiguration(
            configuration,
            configSectionName,
            configureLogging
        );
        ZlogFactory.SetFactory(factory);
        services.AddSingleton(factory);
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        return services;
    }
}
