using System;
using System.IO;

namespace TaskbarFolders.Manager.Services;

/// <summary>
/// Default <see cref="ILauncherPathResolver"/>. Tries two strategies in order:
/// <list type="number">
///   <item>Sibling to the Manager binary (installed / portable layout — installer ships them in one folder).</item>
///   <item>Sibling project's <c>bin</c> tree (dev layout — running <c>dotnet run --project src/TaskbarFolders.Manager</c> means the Launcher lives at <c>../TaskbarFolders.Launcher/bin/&lt;Configuration&gt;/&lt;Tfm&gt;/</c>).</item>
/// </list>
/// Returns the first match or <see langword="null"/>.
/// </summary>
public sealed class LauncherPathResolver : ILauncherPathResolver
{
    /// <summary>File name of the launcher binary the Manager looks for.</summary>
    public const string LauncherFileName = "TaskbarFolders.Launcher.exe";

    /// <inheritdoc/>
    public string? TryResolve()
    {
        var sideBySide = Path.Combine(AppContext.BaseDirectory, LauncherFileName);
        if (File.Exists(sideBySide))
        {
            return sideBySide;
        }

        // Dev layout: walk up from the Manager's bin folder to the solution root, then
        // descend into the Launcher project's bin tree to find a matching configuration.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "TaskbarFolders.sln")))
            {
                var devCandidate = Path.Combine(
                    dir.FullName,
                    "src",
                    "TaskbarFolders.Launcher",
                    "bin",
                    DetectConfiguration(),
                    DetectTargetFramework(),
                    LauncherFileName);

                if (File.Exists(devCandidate))
                {
                    return devCandidate;
                }
                break;
            }
            dir = dir.Parent;
        }

        return null;
    }

    private static string DetectConfiguration()
    {
        // The Manager's bin path is .../bin/<Configuration>/<Tfm>/. Slice the configuration
        // segment so we point at the matching Launcher build (Debug↔Debug, Release↔Release).
        var parts = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);
        return parts.Length >= 2 ? parts[^2] : "Release";
    }

    private static string DetectTargetFramework()
    {
        var parts = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);
        return parts.Length >= 1 ? parts[^1] : "net8.0-windows";
    }
}
