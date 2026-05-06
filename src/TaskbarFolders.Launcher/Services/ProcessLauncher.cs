using System.Diagnostics;

namespace TaskbarFolders.Launcher.Services;

/// <summary>
/// Launches external applications.
/// </summary>
public sealed class ProcessLauncher
{
    /// <summary>
    /// Starts an application with optional arguments.
    /// </summary>
    /// <param name="filePath">Path to the executable.</param>
    /// <param name="arguments">Optional command-line arguments.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Instance method for DI testability")]
    public void Launch(string filePath, string? arguments = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var startInfo = new ProcessStartInfo
        {
            FileName = filePath,
            Arguments = arguments ?? string.Empty,
            UseShellExecute = true,
        };

        Process.Start(startInfo);
    }
}
