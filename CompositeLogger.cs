using Microsoft.Extensions.Logging;

namespace LogExtension;

/// <summary>
/// 组合多个 Logger，同时写入所有日志记录器
/// </summary>
internal sealed class CompositeLogger : ILogger
{
    private readonly ILogger[] _loggers;

    public CompositeLogger(params ILogger[] loggers)
    {
        _loggers = loggers;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        var scopes = new IDisposable?[_loggers.Length];
        for (var i = 0; i < _loggers.Length; i++)
        {
            scopes[i] = _loggers[i].BeginScope(state);
        }
        return new CompositeDisposable(scopes);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        foreach (var logger in _loggers)
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
        foreach (var logger in _loggers)
        {
            logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }

    /// <summary>
    /// 组合多个 IDisposable，统一释放
    /// </summary>
    private sealed class CompositeDisposable : IDisposable
    {
        private readonly IDisposable?[] _disposables;

        public CompositeDisposable(IDisposable?[] disposables)
        {
            _disposables = disposables;
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable?.Dispose();
            }
        }
    }
}
