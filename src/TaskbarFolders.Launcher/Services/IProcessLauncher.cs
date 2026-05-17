namespace TaskbarFolders.Launcher.Services;

/// <summary>
/// Starts an external process for a clicked app entry. Abstracted so the popup view model
/// stays unit-testable without spawning real processes.
/// </summary>
public interface IProcessLauncher
{
    /// <summary>
    /// Launches the target file with the supplied arguments using the shell associations
    /// (so .lnk files resolve and quoted paths Just Work).
    /// </summary>
    /// <param name="path">Absolute path to the executable or shortcut.</param>
    /// <param name="arguments">Optional command-line arguments. Null is treated as empty.</param>
    /// <returns><see langword="true"/> if the process started; <see langword="false"/> on failure.</returns>
    bool Launch(string path, string? arguments);
}
