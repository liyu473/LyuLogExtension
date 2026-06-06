namespace LyuLogExtension.Core;

internal sealed record LogQueryConfiguration(IReadOnlyList<string> Directories)
{
    public static LogQueryConfiguration CreateDefault() =>
        new([Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "logs"))]);
}
