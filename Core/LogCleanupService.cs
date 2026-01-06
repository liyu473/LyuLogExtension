namespace LyuLogExtension.Core;

/// <summary>
/// 日志文件清理服务
/// </summary>
internal class LogCleanupService : IDisposable
{
    private readonly ZLoggerConfig _config;
    private readonly Timer? _timer;
    private bool _disposed;

    public LogCleanupService(ZLoggerConfig config)
    {
        _config = config;

        // 启动时立即清理一次
        CleanupOldLogs();

        // 启用后台定时清理
        if (_config.EnableBackgroundCleanup && _config.RetentionDays > 0)
        {
            _timer = new Timer(
                _ => CleanupOldLogs(),
                null,
                _config.CleanupInterval,
                _config.CleanupInterval);
        }
    }

    /// <summary>
    /// 清理过期日志文件
    /// </summary>
    public void CleanupOldLogs()
    {
        if (_config.RetentionDays <= 0) return;

        var cutoffDate = DateTime.Now.AddDays(-_config.RetentionDays);

        foreach (var output in _config.Outputs)
        {
            CleanupDirectory(output.Path, cutoffDate);
        }
    }

    private static void CleanupDirectory(string path, DateTime cutoffDate)
    {
        try
        {
            var directory = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directory))
            {
                directory = path.TrimEnd('/', '\\');
            }

            if (!Directory.Exists(directory)) return;

            var logFiles = Directory.GetFiles(directory, "*.log", SearchOption.TopDirectoryOnly);

            foreach (var file in logFiles)
            {
                try
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.LastWriteTime < cutoffDate)
                    {
                        fileInfo.Delete();
                    }
                }
                catch
                {
                    // 忽略单个文件删除失败
                }
            }
        }
        catch
        {
            // 忽略目录访问失败
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer?.Dispose();
    }
}
