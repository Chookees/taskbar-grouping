namespace TaskbarFolders.Core.Shortcuts;

/// <summary>
/// Generates per-group Windows shortcuts (.lnk) that can be pinned to the taskbar.
/// Each shortcut points to a single shared host executable, distinguished by command-line
/// arguments and a distinct AUMID so Windows treats it as a separate pinnable identity.
/// </summary>
public interface IShortcutGenerator
{
    /// <summary>
    /// Builds the AUMID Windows uses to identify a group's pinned tile.
    /// Stable for a given <paramref name="groupId"/> so re-saving never breaks an existing pin.
    /// </summary>
    /// <param name="groupId">The group identifier.</param>
    /// <returns>An AUMID of the form <c>TaskbarFolders.Group.{groupId}</c>.</returns>
    string BuildAumid(string groupId);

    /// <summary>
    /// Creates or replaces the shortcut described by <paramref name="request"/>.
    /// Existing files are overwritten atomically.
    /// </summary>
    void Generate(GroupShortcutRequest request);
}
