using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;

namespace TaskbarFolders.Manager.Services;

/// <summary>
/// Default <see cref="ILauncherPathResolver"/>. Probes three layouts in order:
/// <list type="number">
///   <item>
///     <b>Side-by-side</b> — <c>{baseDir}/TaskbarFolders.Launcher.exe</c>. Covers single-folder
///     deployments if anyone ever ships one.
///   </item>
///   <item>
///     <b>Sibling folder</b> — <c>{baseDir}/../Launcher/TaskbarFolders.Launcher.exe</c>.
///     Matches the actual installer + portable ZIP layouts where <c>installer/setup.iss</c>
///     deploys Manager to <c>{app}\Manager</c> and Launcher to <c>{app}\Launcher</c>.
///   </item>
///   <item>
///     <b>Dev walk-up</b> — climbs to <c>TaskbarFolders.sln</c>, then descends into
///     <c>src/TaskbarFolders.Launcher/bin/{Cfg}/{Tfm}/</c>. Activates only when running from
///     <c>dotnet run</c> or the test bin tree.
///   </item>
/// </list>
/// Returns the first match or <see langword="null"/>, logging the probed paths at error level
/// so support logs pinpoint which assumption failed.
/// </summary>
public sealed class LauncherPathResolver : ILauncherPathResolver
{
    /// <summary>File name of the launcher binary the Manager looks for.</summary>
    public const string LauncherFileName = "TaskbarFolders.Launcher.exe";

    /// <summary>Sibling folder name probed for the installer / portable ZIP layout.</summary>
    public const string LauncherFolderName = "Launcher";

    private readonly ILogger<LauncherPathResolver>? _logger;

    /// <summary>Initializes a new instance.</summary>
    /// <param name="logger">Optional logger; diagnostic paths are emitted at error level when no probe matches.</param>
    public LauncherPathResolver(ILogger<LauncherPathResolver>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public string? TryResolve() => TryResolveFrom(AppContext.BaseDirectory);

    /// <summary>
    /// Test hook — runs the probe sequence against an arbitrary base directory so the installer
    /// and portable layouts can be exercised without mutating <see cref="AppContext.BaseDirectory"/>.
    /// </summary>
    internal string? TryResolveFrom(string baseDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseDirectory);

        var probed = new List<string>(3);

        var sideBySide = Path.Combine(baseDirectory, LauncherFileName);
        probed.Add(sideBySide);
        if (File.Exists(sideBySide))
        {
            return sideBySide;
        }

        // Normalise via GetFullPath so the leading ".." is collapsed before the existence
        // check — File.Exists tolerates it, but the diagnostic log should show the resolved form.
        var siblingFolder = Path.GetFullPath(
            Path.Combine(baseDirectory, "..", LauncherFolderName, LauncherFileName));
        probed.Add(siblingFolder);
        if (File.Exists(siblingFolder))
        {
            return siblingFolder;
        }

        var dir = new DirectoryInfo(baseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "TaskbarFolders.sln")))
            {
                var devCandidate = Path.Combine(
                    dir.FullName,
                    "src",
                    "TaskbarFolders.Launcher",
                    "bin",
                    DetectConfiguration(baseDirectory),
                    DetectTargetFramework(baseDirectory),
                    LauncherFileName);

                probed.Add(devCandidate);
                if (File.Exists(devCandidate))
                {
                    return devCandidate;
                }
                break;
            }
            dir = dir.Parent;
        }

        _logger?.LogError(
            "Launcher binary not found. Probed: {Probed}",
            string.Join("; ", probed));
        return null;
    }

    private static string DetectConfiguration(string baseDirectory)
    {
        // The Manager's bin path is .../bin/<Configuration>/<Tfm>/. Slice the configuration
        // segment so we point at the matching Launcher build (Debug↔Debug, Release↔Release).
        var parts = baseDirectory.TrimEnd(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);
        return parts.Length >= 2 ? parts[^2] : "Release";
    }

    private static string DetectTargetFramework(string baseDirectory)
    {
        var parts = baseDirectory.TrimEnd(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);
        return parts.Length >= 1 ? parts[^1] : "net8.0-windows";
    }
}
