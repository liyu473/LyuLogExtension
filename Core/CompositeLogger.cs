using Microsoft.Extensions.Logging;

namespace LogExtension.Core;

/// <summary>
/// 组合多个 Logger，同时写入所有日志记录器
/// </summary>
internal sealed class CompositeLogger(params ILogger[] loggers) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        var scopes = new IDisposable?[loggers.Length];
        for (var i = 0; i < loggers.Length; i++)
        {
            scopes[i] = loggers[i].BeginScope(state);
        }
        return new CompositeDisposable(scopes);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        foreach (var logger in loggers)
        {
            if (logger.IsEnabled(logLevel))
                return true;
        }
        return false;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        foreach (var logger in loggers)
        {
            logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }

    /// <summary>
    /// 组合多个 IDisposable，统一释放
    /// </summary>
    private sealed class CompositeDisposable(IDisposable?[] disposables) : IDisposable
    {
        public void Dispose()
        {
            foreach (var disposable in disposables)
            {
                disposable?.Dispose();
            }
        }
    }
}
