namespace TaskbarFolders.Core.Shortcuts;

/// <summary>
/// Description of a pinnable shortcut to generate. Input to <see cref="IShortcutGenerator"/>.
/// </summary>
/// <param name="GroupId">Group identifier — drives the AUMID and the shortcut filename.</param>
/// <param name="DisplayName">Friendly name surfaced as the shortcut's description (shown on hover and used as the pinned tile label).</param>
/// <param name="TargetExePath">Absolute path to the host executable invoked when the user clicks the shortcut. Typically <c>Launcher.exe</c>.</param>
/// <param name="IconPath">Absolute path to a multi-resolution <c>.ico</c> file. Embedded by index 0.</param>
/// <param name="ShortcutPath">Absolute destination for the generated <c>.lnk</c>.</param>
public sealed record GroupShortcutRequest(
    string GroupId,
    string DisplayName,
    string TargetExePath,
    string IconPath,
    string ShortcutPath);
