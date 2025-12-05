using Microsoft.Extensions.Logging;

namespace LogExtension.Core;

/// <summary>
/// 组合多个 LoggerFactory，创建的 Logger 会同时写入所有工厂
/// </summary>
internal sealed class CompositeLoggerFactory(params ILoggerFactory[] factories) : ILoggerFactory
{
    public void AddProvider(ILoggerProvider provider)
    {
        foreach (var factory in factories)
        {
            factory.AddProvider(provider);
        }
    }

    public ILogger CreateLogger(string categoryName)
    {
        var loggers = new ILogger[factories.Length];
        for (var i = 0; i < factories.Length; i++)
        {
            loggers[i] = factories[i].CreateLogger(categoryName);
        }
        return new CompositeLogger(loggers);
    }

    public void Dispose()
    {
        foreach (var factory in factories)
        {
            factory.Dispose();
        }
    }
}
