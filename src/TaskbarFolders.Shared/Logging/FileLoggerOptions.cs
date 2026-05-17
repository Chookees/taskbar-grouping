using Microsoft.Extensions.Logging;

namespace TaskbarFolders.Shared.Logging;

/// <summary>
/// Configuration for <see cref="FileLoggerProvider"/>.
/// </summary>
public sealed class FileLoggerOptions
{
    /// <summary>Directory that will receive rotated log files. Created if missing.</summary>
    public string Directory { get; set; } = string.Empty;

    /// <summary>File name prefix; the date stamp and <c>.log</c> extension are appended.</summary>
    public string FilePrefix { get; set; } = "log";

    /// <summary>How many days of log files to retain. Older files are deleted at provider construction.</summary>
    public int RetainDays { get; set; } = 14;

    /// <summary>Minimum level captured by the file sink. Defaults to <see cref="LogLevel.Information"/>.</summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Information;
}
