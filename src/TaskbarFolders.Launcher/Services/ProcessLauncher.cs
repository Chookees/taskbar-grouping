using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;

namespace TaskbarFolders.Launcher.Services;

/// <summary>
/// Default <see cref="IProcessLauncher"/>. Uses <see cref="Process.Start(ProcessStartInfo)"/>
/// with <c>UseShellExecute = true</c> so the OS resolves <c>.lnk</c> targets, applies
/// associations, and quotes paths correctly. The working directory is set to the
/// target's containing folder so apps that load resources by relative path work.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class ProcessLauncher : IProcessLauncher
{
    private readonly ILogger<ProcessLauncher>? _logger;

    /// <summary>Initializes a new instance.</summary>
    /// <param name="logger">Optional logger.</param>
    public ProcessLauncher(ILogger<ProcessLauncher>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public bool Launch(string path, string? arguments)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = path,
            Arguments = arguments ?? string.Empty,
            UseShellExecute = true,
            WorkingDirectory = SafeGetDirectoryName(path),
        };

        try
        {
            using var process = Process.Start(startInfo);
            if (process is null)
            {
                _logger?.LogWarning("Process.Start returned null for {Path}.", path);
                return false;
            }
            return true;
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or FileNotFoundException)
        {
            _logger?.LogWarning(ex, "Failed to launch {Path}.", path);
            return false;
        }
    }

    private static string SafeGetDirectoryName(string path)
    {
        try
        {
            return Path.GetDirectoryName(path) ?? string.Empty;
        }
        catch (ArgumentException)
        {
            return string.Empty;
        }
    }
}
