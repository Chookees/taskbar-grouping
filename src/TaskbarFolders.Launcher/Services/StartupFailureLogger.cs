using System;
using System.Globalization;
using System.IO;
using System.Text;
using TaskbarFolders.Shared.Configuration;

namespace TaskbarFolders.Launcher.Services;

/// <summary>
/// Last-chance file logger for launcher failures that occur before the DI container
/// (and with it the shared file-logging pipeline) is available, or on paths where it
/// can no longer be trusted (unhandled-exception handlers). Writes the same line format
/// to the same daily <c>launcher-yyyy-MM-dd.log</c> file the DI logger uses, so all
/// startup evidence lands in one place.
/// </summary>
/// <remarks>
/// Launcher invariant: every early-exit or failure path must reach the file log.
/// <c>Trace</c>-only diagnostics are invisible in published Release builds and have
/// hidden startup crashes from the log entirely (v0.4.x popup regression).
/// </remarks>
internal static class StartupFailureLogger
{
    /// <summary>
    /// Appends one <c>[Error]</c> line (plus optional exception) to today's launcher log.
    /// Never throws — last-chance logging must not take the process down or mask the
    /// original failure.
    /// </summary>
    /// <param name="message">Human-readable failure description, including the exit code.</param>
    /// <param name="exception">Original failure, if any; logged with full stack.</param>
    internal static void Log(string message, Exception? exception = null)
    {
        try
        {
            var directory = new AppDataPathProvider().LogsDirectory;
            Directory.CreateDirectory(directory);

            var builder = new StringBuilder();
            builder.Append(DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            builder.Append(" [Error] ");
            builder.Append(typeof(App).FullName);
            builder.Append(": ");
            builder.Append(message);
            if (exception is not null)
            {
                builder.AppendLine();
                builder.Append(exception);
            }
            builder.AppendLine();

            var today = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            File.AppendAllText(Path.Combine(directory, $"launcher-{today}.log"), builder.ToString());
        }
        catch (Exception)
        {
            // Swallow by design — see method summary.
        }
    }
}
