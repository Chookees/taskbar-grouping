using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TaskbarFolders.Shared.Logging;

/// <summary>
/// <see cref="ILoggingBuilder"/> extensions for wiring the TaskbarFolders file sink.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Registers <see cref="FileLoggerProvider"/> with the supplied directory and file-name prefix.
    /// </summary>
    /// <param name="builder">Logging builder from <c>IHostBuilder.ConfigureLogging</c> or <c>IServiceCollection.AddLogging</c>.</param>
    /// <param name="directory">Absolute path to the log directory. Created if missing.</param>
    /// <param name="filePrefix">File-name prefix; the date stamp and <c>.log</c> extension are appended automatically.</param>
    /// <param name="configure">Optional callback for further customisation (e.g. retention, minimum level).</param>
    public static ILoggingBuilder AddTaskbarFoldersFileLogging(
        this ILoggingBuilder builder,
        string directory,
        string filePrefix,
        Action<FileLoggerOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePrefix);

        builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
        builder.Services.Configure<FileLoggerOptions>(options =>
        {
            options.Directory = directory;
            options.FilePrefix = filePrefix;
            configure?.Invoke(options);
        });

        return builder;
    }
}
