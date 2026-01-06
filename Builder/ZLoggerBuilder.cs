using LyuLogExtension.Core;
using LyuLogExtension.Extensions;
using Microsoft.Extensions.Logging;

namespace LyuLogExtension.Builder;

/// <summary>
/// ZLogger 配置构建器
/// </summary>
public class ZLoggerBuilder
{
    internal ZLoggerConfig Config { get; } = new();

    /// <summary>
    /// 构建 LoggerFactory
    /// </summary>
    public ILoggerFactory Build()
    {
        var factories = new List<ILoggerFactory>();

        // 为每个输出配置创建工厂
        foreach (var output in Config.Outputs)
        {
            factories.Add(output.CreateFactoryForOutput(Config));
        }

        // 如果没有配置任何输出，添加默认输出
        if (factories.Count == 0)
        {
            var defaultOutput = new LogOutputConfig { Path = "logs/" };
            factories.Add(defaultOutput.CreateFactoryForOutput(Config));
        }

        // 添加控制台工厂
        if (Config.EnableConsole || Config.EnableConsoleWithDetails)
        {
            factories.Add(Config.CreateConsoleFactory());
        }

        // 启动日志清理服务
        if (Config.RetentionDays > 0)
        {
            var cleanupService = new LogCleanupService(Config);
            // 将清理服务附加到 CompositeLoggerFactory 以便统一释放
            return new CompositeLoggerFactory([.. factories], cleanupService);
        }

        return new CompositeLoggerFactory([.. factories]);
    }
}
