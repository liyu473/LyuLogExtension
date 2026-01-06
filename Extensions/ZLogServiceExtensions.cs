using LyuLogExtension.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LyuLogExtension.Extensions;

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
    /// 添加 ZLogger 日志服务（使用链式配置）
    /// </summary>
    public static IServiceCollection AddZLogger(
        this IServiceCollection services,
        Action<ZLoggerBuilder> configure)
    {
        var builder = new ZLoggerBuilder();
        configure(builder);

        var factory = builder.Build();
        ZLogFactory.SetFactory(factory);
        return services.RegisterFactory(factory);
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
    /// 添加 ZLogger 日志服务（从 IConfiguration 读取配置）
    /// </summary>
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
    /// 添加 ZLogger 日志服务（从 IConfiguration 读取配置并覆盖）
    /// </summary>
    public static IServiceCollection AddZLogger(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<ZLoggerBuilder> configure,
        string configSectionName = "ZLogger")
    {
        var builder = new ZLoggerBuilder();
        builder.FromConfiguration(configuration, configSectionName);
        configure(builder);

        var factory = builder.Build();
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
