using Microsoft.Extensions.Logging;

namespace LyuLogExtension;

public sealed record LogQueryEntry(
    DateTime Timestamp,
    LogLevel Level,
    string Category,
    int LineNumber,
    string Message
);
