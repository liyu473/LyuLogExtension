using Microsoft.Extensions.Logging;
using ZLogger.Providers;

namespace LyuLogExtension.Core;

/// <summary>
/// ZLogger 配置选项（内部使用）
/// </summary>
internal class ZLoggerConfig
{
    /// <summary>
    /// 日志输出配置列表
    /// </summary>
    public List<LogOutputConfig> Outputs { get; set; } = [];

    /// <summary>
    /// 全局日志滚动间隔（默认：每小时）
    /// </summary>
    public RollingInterval GlobalRollingInterval { get; set; } = RollingInterval.Hour;

    /// <summary>
    /// 全局单个日志文件最大大小 KB（默认：2048KB = 2MB）
    /// </summary>
    public int GlobalRollingSizeKB { get; set; } = 2048;

    /// <summary>
    /// 日志级别过滤规则
    /// Key: 类别名称（如 "System.Net.Http"）
    /// Value: 最低日志级别
    /// </summary>
    public Dictionary<string, LogLevel> CategoryFilters { get; set; } = [];

    /// <summary>
    /// 额外的日志配置回调（如添加控制台、Debug 输出等）
    /// </summary>
    public Action<ILoggingBuilder>? AdditionalConfiguration { get; set; }

    /// <summary>
    /// 是否启用控制台输出
    /// </summary>
    public bool EnableConsole { get; set; }

    /// <summary>
    /// 是否启用控制台详情输出（带类名）
    /// </summary>
    public bool EnableConsoleWithDetails { get; set; }

    /// <summary>
    /// 控制台独立过滤器（优先于全局过滤器）
    /// </summary>
    public Dictionary<string, LogLevel> ConsoleCategoryFilters { get; set; } = [];

    /// <summary>
    /// 控制台是否使用全局过滤器（默认 true）
    /// </summary>
    public bool ConsoleUseGlobalFilters { get; set; } = true;

    /// <summary>
    /// 日志文件保留天数（默认：7天，0 表示不清理）
    /// </summary>
    public int RetentionDays { get; set; } = 7;

    /// <summary>
    /// 是否启用后台定时清理（默认：true）
    /// </summary>
    public bool EnableBackgroundCleanup { get; set; } = true;

    /// <summary>
    /// 后台清理间隔（默认：1小时）
    /// </summary>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(1);

    internal LogQueryConfiguration CreateQueryConfiguration()
    {
        var directories = Outputs.Count == 0
            ? [Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "logs"))]
            : NormalizeDirectories(
                [.. Outputs.Select(static x => ResolveOutputDirectory(x.Path))]
            );

        return new LogQueryConfiguration(directories);
    }

    private static string[] NormalizeDirectories(IReadOnlyList<string> directories)
    {
        var orderedDirectories = directories
            .Where(static x => !string.IsNullOrWhiteSpace(x))
            .Select(static x => Path.GetFullPath(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static x => x.Length)
            .ThenBy(static x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var normalized = new List<string>(orderedDirectories.Length);
        foreach (var directory in orderedDirectories)
        {
            if (normalized.Any(existing => IsSameOrChildPath(existing, directory)))
            {
                continue;
            }

            normalized.Add(directory);
        }

        return [.. normalized];
    }

    private static bool IsSameOrChildPath(string parentPath, string candidatePath)
    {
        if (string.Equals(parentPath, candidatePath, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var normalizedParent = parentPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var normalizedCandidate = candidatePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        return normalizedCandidate.StartsWith(normalizedParent, StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveOutputDirectory(string path)
    {
        var trimmedPath = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (string.IsNullOrWhiteSpace(trimmedPath))
        {
            trimmedPath = "logs";
        }

        var directory = Path.GetDirectoryName(trimmedPath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            directory = trimmedPath;
        }

        return Path.GetFullPath(directory, AppContext.BaseDirectory);
    }
}
