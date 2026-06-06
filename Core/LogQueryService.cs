using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace LyuLogExtension.Core;

internal sealed class LogQueryService(LogQueryConfiguration configuration) : ILogQueryService
{
    private static readonly Regex LogEntryRegex = new(
        @"^(?<timestamp>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}) \[(?<level>[A-Z]{3})\] \[(?<category>.*?):(?<lineNumber>\d+)\] (?<message>.*)$",
        RegexOptions.Compiled
    );

    private static readonly Regex LogFileNameRegex = new(
        @"^(?<timestamp>\d{4}-\d{2}-\d{2}-\d{2})_\d{3}\.log$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public Task<LogQueryPage> QueryAsync(
        LogQueryOptions options,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.StartTime.HasValue && options.EndTime.HasValue && options.StartTime > options.EndTime)
        {
            throw new ArgumentException("StartTime cannot be greater than EndTime.", nameof(options));
        }

        if (options.PageNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "PageNumber must be greater than 0.");
        }

        if (options.PageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "PageSize must be greater than 0.");
        }

        return Task.Run(() => QueryCore(options, cancellationToken), cancellationToken);
    }

    private LogQueryPage QueryCore(LogQueryOptions options, CancellationToken cancellationToken)
    {
        var levelFilter = options.Levels is null || options.Levels.Count == 0
            ? null
            : options.Levels.ToHashSet();
        var dedup = new HashSet<LogQueryIdentity>();
        var pageItems = new List<LogQueryEntry>(options.PageSize);
        var skipCount = (options.PageNumber - 1) * options.PageSize;
        var matchedUniqueCount = 0;
        var hasMore = false;

        foreach (var filePath in EnumerateCandidateFiles(options))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                foreach (var entry in ReadEntriesFromFile(filePath, options, levelFilter, cancellationToken))
                {
                    var identity = new LogQueryIdentity(
                        entry.Timestamp,
                        entry.Level,
                        entry.Category,
                        entry.LineNumber,
                        entry.Message
                    );

                    if (!dedup.Add(identity))
                    {
                        continue;
                    }

                    if (matchedUniqueCount < skipCount)
                    {
                        matchedUniqueCount++;
                        continue;
                    }

                    if (pageItems.Count < options.PageSize)
                    {
                        pageItems.Add(entry);
                        matchedUniqueCount++;
                        continue;
                    }

                    hasMore = true;
                    break;
                }
            }
            catch (IOException)
            {
                // Skip files that are temporarily unavailable while rolling or writing.
            }
            catch (UnauthorizedAccessException)
            {
                // Skip files that cannot be opened at query time.
            }

            if (hasMore)
            {
                break;
            }
        }

        return new LogQueryPage
        {
            Items = pageItems,
            PageNumber = options.PageNumber,
            PageSize = options.PageSize,
            HasMore = hasMore
        };
    }

    private IEnumerable<string> EnumerateCandidateFiles(LogQueryOptions options)
    {
        var files = new List<LogFileCandidate>();

        foreach (var directory in configuration.Directories)
        {
            if (!Directory.Exists(directory))
            {
                continue;
            }

            foreach (var filePath in Directory.EnumerateFiles(directory, "*.log", SearchOption.TopDirectoryOnly))
            {
                if (!ShouldReadFile(filePath, options))
                {
                    continue;
                }

                files.Add(new LogFileCandidate(filePath, GetFileSortTime(filePath)));
            }
        }

        var orderedFiles = options.SortDescending
            ? files
                .OrderByDescending(static x => x.SortTime)
                .ThenByDescending(static x => x.Path, StringComparer.OrdinalIgnoreCase)
            : files
                .OrderBy(static x => x.SortTime)
                .ThenBy(static x => x.Path, StringComparer.OrdinalIgnoreCase);

        foreach (var file in orderedFiles)
        {
            yield return file.Path;
        }
    }

    private static bool ShouldReadFile(string filePath, LogQueryOptions options)
    {
        if (!options.StartTime.HasValue && !options.EndTime.HasValue)
        {
            return true;
        }

        var fileStart = GetFileSortTime(filePath);
        var fileEnd = fileStart.AddHours(1);

        if (options.StartTime.HasValue && fileEnd < options.StartTime.Value)
        {
            return false;
        }

        if (options.EndTime.HasValue && fileStart > options.EndTime.Value)
        {
            return false;
        }

        return true;
    }

    private static DateTime GetFileSortTime(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var match = LogFileNameRegex.Match(fileName);

        if (
            match.Success &&
            DateTime.TryParseExact(
                match.Groups["timestamp"].Value,
                "yyyy-MM-dd-HH",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var fileStart
            )
        )
        {
            return fileStart;
        }

        return File.GetLastWriteTime(filePath);
    }

    private static IEnumerable<LogQueryEntry> ReadEntriesFromFile(
        string filePath,
        LogQueryOptions options,
        HashSet<LogLevel>? levelFilter,
        CancellationToken cancellationToken
    )
    {
        using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete
        );
        using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);

        ParsedLogEntry? currentEntry = null;
        StringBuilder? messageBuilder = null;
        var fileEntries = new List<LogQueryEntry>();

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = reader.ReadLine();
            if (line is null)
            {
                continue;
            }

            var parsed = TryParseEntryLine(line);
            if (parsed is not null)
            {
                FlushCurrentEntry(currentEntry, messageBuilder, options, levelFilter, fileEntries);
                currentEntry = parsed;
                messageBuilder = new StringBuilder(parsed.Message);
                continue;
            }

            if (currentEntry is null || messageBuilder is null)
            {
                continue;
            }

            if (messageBuilder.Length > 0)
            {
                messageBuilder.AppendLine();
            }

            messageBuilder.Append(line);
        }

        FlushCurrentEntry(currentEntry, messageBuilder, options, levelFilter, fileEntries);

        if (!options.SortDescending)
        {
            for (var i = 0; i < fileEntries.Count; i++)
            {
                yield return fileEntries[i];
            }

            yield break;
        }

        for (var i = fileEntries.Count - 1; i >= 0; i--)
        {
            yield return fileEntries[i];
        }
    }

    private static void FlushCurrentEntry(
        ParsedLogEntry? currentEntry,
        StringBuilder? messageBuilder,
        LogQueryOptions options,
        HashSet<LogLevel>? levelFilter,
        List<LogQueryEntry> entries
    )
    {
        if (currentEntry is null || messageBuilder is null)
        {
            return;
        }

        var entry = new LogQueryEntry(
            currentEntry.Timestamp,
            currentEntry.Level,
            currentEntry.Category,
            currentEntry.LineNumber,
            messageBuilder.ToString()
        );

        if (!MatchesFilter(entry, options, levelFilter))
        {
            return;
        }

        entries.Add(entry);
    }

    private static bool MatchesFilter(
        LogQueryEntry entry,
        LogQueryOptions options,
        HashSet<LogLevel>? levelFilter
    )
    {
        if (options.StartTime.HasValue && entry.Timestamp < options.StartTime.Value)
        {
            return false;
        }

        if (options.EndTime.HasValue && entry.Timestamp > options.EndTime.Value)
        {
            return false;
        }

        if (levelFilter is not null && !levelFilter.Contains(entry.Level))
        {
            return false;
        }

        return true;
    }

    private static ParsedLogEntry? TryParseEntryLine(string line)
    {
        var match = LogEntryRegex.Match(line);
        if (!match.Success)
        {
            return null;
        }

        if (
            !DateTime.TryParseExact(
                match.Groups["timestamp"].Value,
                "yyyy-MM-dd HH:mm:ss.fff",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var timestamp
            )
        )
        {
            return null;
        }

        if (!TryMapLogLevel(match.Groups["level"].Value, out var level))
        {
            return null;
        }

        if (!int.TryParse(match.Groups["lineNumber"].Value, out var lineNumber))
        {
            lineNumber = 0;
        }

        return new ParsedLogEntry(
            timestamp,
            level,
            match.Groups["category"].Value,
            lineNumber,
            match.Groups["message"].Value
        );
    }

    private static bool TryMapLogLevel(string levelText, out LogLevel level)
    {
        level = levelText switch
        {
            "TRC" => LogLevel.Trace,
            "DBG" => LogLevel.Debug,
            "INF" => LogLevel.Information,
            "WRN" => LogLevel.Warning,
            "ERR" => LogLevel.Error,
            "CRT" => LogLevel.Critical,
            _ => LogLevel.None,
        };

        return level != LogLevel.None;
    }

    private sealed record ParsedLogEntry(
        DateTime Timestamp,
        LogLevel Level,
        string Category,
        int LineNumber,
        string Message
    );

    private sealed record LogQueryIdentity(
        DateTime Timestamp,
        LogLevel Level,
        string Category,
        int LineNumber,
        string Message
    );

    private sealed record LogFileCandidate(string Path, DateTime SortTime);
}
