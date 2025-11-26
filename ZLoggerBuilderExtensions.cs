using Microsoft.Extensions.Logging;
using Utf8StringInterpolation;
using ZLogger;

namespace LogExtension;

/// <summary>
/// ZLogger 构建器扩展方法
/// </summary>
public static class ZLoggerBuilderExtensions
{
    /// <summary>
    /// 添加带时间戳格式的控制台输出
    /// </summary>
    /// <param name="builder">日志构建器</param>
    /// <returns>日志构建器</returns>
    public static ILoggingBuilder AddZLoggerConsoleWithTimestamp(this ILoggingBuilder builder)
    {
        return builder.AddZLoggerConsole(options =>
        {
            options.UsePlainTextFormatter(formatter =>
            {
                formatter.SetPrefixFormatter(
                    $"{0:yyyy-MM-dd HH:mm:ss.fff} [{1:short}] ",
                    (in MessageTemplate template, in LogInfo info) =>
                        template.Format(info.Timestamp, info.LogLevel)
                );
            });
        });
    }

    /// <summary>
    /// 添加带时间戳和类名的控制台输出
    /// </summary>
    /// <param name="builder">日志构建器</param>
    /// <returns>日志构建器</returns>
    public static ILoggingBuilder AddZLoggerConsoleWithDetails(this ILoggingBuilder builder)
    {
        return builder.AddZLoggerConsole(options =>
        {
            options.UsePlainTextFormatter(formatter =>
            {
                formatter.SetPrefixFormatter(
                    $"{0:yyyy-MM-dd HH:mm:ss.fff} [{1:short}] [{2}] ",
                    (in MessageTemplate template, in LogInfo info) =>
                        template.Format(info.Timestamp, info.LogLevel, info.Category)
                );
            });
        });
    }
}
