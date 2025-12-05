using LogExtension.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogExtension.Builder;

/// <summary>
/// ZLoggerBuilder 配置文件加载扩展
/// </summary>
public static partial class ZLoggerBuilderConfigExtensions
{
    /// <summary>
    /// 添加额外的日志配置
    /// </summary>
    public static ZLoggerBuilder WithAdditionalConfiguration(this ZLoggerBuilder builder, Action<ILoggingBuilder> configure)
    {
        var original = builder.Config.AdditionalConfiguration;
        builder.Config.AdditionalConfiguration = b =>
        {
            original?.Invoke(b);
            configure(b);
        };
        return builder;
    }

    /// <summary>
    /// 从 IConfiguration 加载配置
    /// </summary>
    public static ZLoggerBuilder FromConfiguration(this ZLoggerBuilder builder, IConfiguration configuration, string sectionName = "ZLogger")
    {
        var loadedConfig = configuration.ParseFromConfiguration(sectionName);

        // 合并全局配置
        builder.Config.GlobalRollingInterval = loadedConfig.GlobalRollingInterval;
        builder.Config.GlobalRollingSizeKB = loadedConfig.GlobalRollingSizeKB;

        // 合并输出配置
        foreach (var output in loadedConfig.Outputs)
        {
            builder.Config.Outputs.Add(output);
        }

        // 合并过滤器
        foreach (var filter in loadedConfig.CategoryFilters)
        {
            builder.Config.CategoryFilters[filter.Key] = filter.Value;
        }

        return builder;
    }
}
