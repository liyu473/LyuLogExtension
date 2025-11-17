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
        ILoggerFactory customFactory)
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
        Action<ZLoggerConfig> configure)
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
    /// 添加 ZLogger 日志服务，使用配置对象
    /// </summary>
    public static IServiceCollection AddZLogger(
        this IServiceCollection services,
        ZLoggerConfig config)
    {
        var factory = ZlogFactory.CreateFactoryWithConfig(config);
        ZlogFactory.SetFactory(factory);
        services.AddSingleton(factory);
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        
        return services;
    }
}
