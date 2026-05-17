using System;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TaskbarFolders.Shared.Logging;

/// <summary>
/// <see cref="ILoggerProvider"/> that produces <see cref="FileLogger"/> instances writing to
/// daily-rotated files under <see cref="FileLoggerOptions.Directory"/>.
/// Prunes log files older than <see cref="FileLoggerOptions.RetainDays"/> when
/// <see cref="StartBackgroundPrune"/> is called (App.OnStartup post-Show); pre-v0.4 the prune
/// ran inside the ctor and added ~5-20 ms to startup.
/// </summary>
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly FileLoggerOptions _options;
    private readonly object _writeLock = new();

    /// <summary>Initializes a new instance and prepares the target directory.</summary>
    /// <param name="options">Provider options resolved from the DI container.</param>
    public FileLoggerProvider(IOptions<FileLoggerOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options.Value;
        if (string.IsNullOrWhiteSpace(_options.Directory))
        {
            throw new ArgumentException(
                $"{nameof(FileLoggerOptions)}.{nameof(FileLoggerOptions.Directory)} must be set.",
                nameof(options));
        }

        Directory.CreateDirectory(_options.Directory);
    }

    /// <summary>
    /// Schedules a background sweep of stale log files. Fire-and-forget. Call once from
    /// App.OnStartup after the main window has been shown so the file IO does not block
    /// the first paint. IOException is swallowed; next launch will retry.
    /// </summary>
    public void StartBackgroundPrune() =>
        System.Threading.Tasks.Task.Run(() =>
        {
            try
            {
                PruneOldFiles();
            }
            catch (IOException)
            {
                // Next launch will retry; logging the exception here would risk recursion.
            }
        });

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName) =>
        new FileLogger(categoryName, _options, _writeLock);

    /// <inheritdoc/>
    public void Dispose()
    {
        // FileLogger holds no unmanaged resources — nothing to dispose.
    }

    private void PruneOldFiles()
    {
        if (_options.RetainDays <= 0)
        {
            return;
        }

        // Defensive: deferred prune means the directory could have been deleted between
        // ctor and Task.Run completion. EnumerateFiles would throw DirectoryNotFoundException
        // and the StartBackgroundPrune wrapper only swallows IOException.
        if (!Directory.Exists(_options.Directory))
        {
            return;
        }

        var cutoff = DateTime.UtcNow.AddDays(-_options.RetainDays);
        var pattern = $"{_options.FilePrefix}-*.log";

        foreach (var file in Directory.EnumerateFiles(_options.Directory, pattern))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var datePart = name[(_options.FilePrefix.Length + 1)..];
            if (DateTime.TryParseExact(
                datePart,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out var fileDate) && fileDate < cutoff)
            {
                try
                {
                    File.Delete(file);
                }
                catch (IOException)
                {
                    // File in use by another process — skip; will be retried on the next run.
                }
            }
        }
    }
}
