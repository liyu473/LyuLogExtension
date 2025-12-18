using Microsoft.Extensions.Logging;
using ZLogger;

namespace LogExtension.Extensions;

/// <summary>
/// ZLogger ILoggingBuilder 扩展方法（内部使用）
/// </summary>
internal static class ZLoggerLoggingBuilderExtensions
{
    /// <summary>
    /// 添加带时间戳格式的控制台输出（彩色）
    /// </summary>
    public static ILoggingBuilder AddZLoggerConsoleWithTimestamp(this ILoggingBuilder builder)
    {
        return builder.AddZLoggerConsole(options =>
        {
            options.UsePlainTextFormatter(formatter =>
            {
                // 时间(暗灰) [级别(彩色)] 消息(白色)
                formatter.SetPrefixFormatter(
                    $"{0}{1:local-longdate} {2}[{3:short}]{4} ",
                    (in MessageTemplate template, in LogInfo info) =>
                        template.Format(DimGray, info.Timestamp, GetLogLevelColor(info.LogLevel), info.LogLevel, White));

                formatter.SetSuffixFormatter($"{0}", (in MessageTemplate template, in LogInfo info) =>
                    template.Format(Reset));
            });
        });
    }

    /// <summary>
    /// 添加带时间戳和类名的控制台输出（彩色）
    /// </summary>
    public static ILoggingBuilder AddZLoggerConsoleWithDetails(this ILoggingBuilder builder)
    {
        return builder.AddZLoggerConsole(options =>
        {
            options.UsePlainTextFormatter(formatter =>
            {
                // 时间(暗灰) [级别(彩色)] [类名(青色)] 消息(白色)
                formatter.SetPrefixFormatter(
                    $"{0}{1:local-longdate} {2}[{3:short}]{4} [{5}]{6} ",
                    (in MessageTemplate template, in LogInfo info) =>
                        template.Format(DimGray, info.Timestamp, GetLogLevelColor(info.LogLevel), info.LogLevel, Cyan, info.Category, White));

                formatter.SetSuffixFormatter($"{0}", (in MessageTemplate template, in LogInfo info) =>
                    template.Format(Reset));
            });
        });
    }

    // ANSI 颜色常量
    private const string Reset = "\u001b[0m";
    private const string DimGray = "\u001b[90m";   // 暗灰色 - 时间戳
    private const string White = "\u001b[97m";    // 亮白色 - 消息内容
    private const string Cyan = "\u001b[36m";     // 青色 - 类名

    /// <summary>
    /// 根据日志级别获取 ANSI 颜色代码（VSCode 风格）
    /// </summary>
    private static string GetLogLevelColor(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Trace => "\u001b[90m",           // 暗灰色
        LogLevel.Debug => "\u001b[34m",           // 蓝色
        LogLevel.Information => "\u001b[32m",    // 绿色
        LogLevel.Warning => "\u001b[33m",         // 黄色
        LogLevel.Error => "\u001b[91m",           // 亮红色
        LogLevel.Critical => "\u001b[95;1m",     // 亮紫色+粗体
        _ => Reset
    };
}
