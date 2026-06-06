namespace LyuLogExtension;

public sealed class LogQueryPage
{
    public required IReadOnlyList<LogQueryEntry> Items { get; init; }

    public required int PageNumber { get; init; }

    public required int PageSize { get; init; }

    public required bool HasMore { get; init; }
}
