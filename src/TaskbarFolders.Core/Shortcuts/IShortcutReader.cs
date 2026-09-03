namespace TaskbarFolders.Core.Shortcuts;

/// <summary>
/// Reads back the AppUserModelID stamped on a shortcut — the read counterpart to
/// <see cref="IShortcutGenerator"/>.
/// </summary>
/// <remarks>
/// Exists so a pin attempt can be verified rather than believed. Windows copies a shortcut
/// into its own pinned-items folder under a name it chooses, so the only reliable way to
/// tell whether a particular group actually landed on the taskbar is to compare AUMIDs.
/// </remarks>
public interface IShortcutReader
{
    /// <summary>
    /// Reads the <c>PKEY_AppUserModel_ID</c> of the shortcut at <paramref name="shortcutPath"/>.
    /// </summary>
    /// <param name="shortcutPath">Full path to a <c>.lnk</c> file.</param>
    /// <returns>
    /// The stamped AppUserModelID, or <see langword="null"/> when the file carries none, does
    /// not exist, or cannot be read. Never throws: callers use this to make a diagnosis, and a
    /// diagnosis that fails must not escalate into a failure of the operation being diagnosed.
    /// </returns>
    string? TryReadAumid(string shortcutPath);
}
