using Microsoft.Extensions.Logging;

namespace LyuLogExtension;

public sealed class LogQueryOptions
{
    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public IReadOnlyCollection<LogLevel>? Levels { get; set; }

    public bool SortDescending { get; set; } = true;

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 200;
}
