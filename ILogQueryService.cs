namespace LyuLogExtension;

public interface ILogQueryService
{
    Task<LogQueryPage> QueryAsync(
        LogQueryOptions options,
        CancellationToken cancellationToken = default
    );
}
