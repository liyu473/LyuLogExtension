using Microsoft.Extensions.Logging;

namespace LyuLogExtension.Core;

/// <summary>
/// 组合多个 LoggerFactory，创建的 Logger 会同时写入所有工厂
/// </summary>
internal sealed class CompositeLoggerFactory : ILoggerFactory
{
    private readonly ILoggerFactory[] _factories;
    private readonly LogCleanupService? _cleanupService;

    public CompositeLoggerFactory(ILoggerFactory[] factories, LogCleanupService? cleanupService = null)
    {
        _factories = factories;
        _cleanupService = cleanupService;
    }

    public void AddProvider(ILoggerProvider provider)
    {
        foreach (var factory in _factories)
        {
            factory.AddProvider(provider);
        }
    }

    public ILogger CreateLogger(string categoryName)
    {
        var loggers = new ILogger[_factories.Length];
        for (var i = 0; i < _factories.Length; i++)
        {
            loggers[i] = _factories[i].CreateLogger(categoryName);
        }
        return new CompositeLogger(loggers);
    }

    public void Dispose()
    {
        _cleanupService?.Dispose();

        foreach (var factory in _factories)
        {
            factory.Dispose();
        }
    }
}
