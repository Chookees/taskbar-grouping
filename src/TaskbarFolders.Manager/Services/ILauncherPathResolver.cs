namespace TaskbarFolders.Manager.Services;

/// <summary>
/// Locates the host <c>TaskbarFolders.Launcher.exe</c> binary that per-group shortcuts target.
/// Abstracted from the default filesystem resolver so unit tests can supply a fixture path.
/// </summary>
public interface ILauncherPathResolver
{
    /// <summary>
    /// Resolves an absolute path to the launcher executable, or <see langword="null"/> if it
    /// cannot be found. <see langword="null"/> is a non-fatal signal — the Manager surfaces an
    /// inline error rather than crashing the save flow.
    /// </summary>
    string? TryResolve();
}
