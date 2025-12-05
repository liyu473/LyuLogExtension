using LogExtension.Core;
using LogExtension.Extensions;
using Microsoft.Extensions.Logging;

namespace LogExtension.Builder;

/// <summary>
/// ZLogger 配置构建器
/// </summary>
public class ZLoggerBuilder
{
    internal ZLoggerConfig Config { get; } = new();
    internal bool EnableConsole { get; set; }
    internal bool EnableConsoleWithDetails { get; set; }

    /// <summary>
    /// 构建 LoggerFactory
    /// </summary>
    public ILoggerFactory Build()
    {
        if (EnableConsole || EnableConsoleWithDetails)
        {
            var originalConfig = Config.AdditionalConfiguration;
            Config.AdditionalConfiguration = builder =>
            {
                originalConfig?.Invoke(builder);
                if (EnableConsoleWithDetails)
                    builder.AddZLoggerConsoleWithDetails();
                else if (EnableConsole)
                    builder.AddZLoggerConsoleWithTimestamp();
            };
        }

        return ZLogFactory.CreateFactoryWithConfig(Config);
    }
}
