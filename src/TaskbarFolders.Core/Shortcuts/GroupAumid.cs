using System;

namespace TaskbarFolders.Core.Shortcuts;

/// <summary>
/// Single source of truth for the AppUserModelID format used by both the shortcut writer
/// (Manager-side, when generating the .lnk) and the launcher process (when stamping its
/// own AUMID via <c>SetCurrentProcessExplicitAppUserModelID</c>). Both ends MUST agree on
/// this string or Windows will not consider the running launcher window an instance of
/// the pinned tile and will leave the pin "ghost-empty".
/// </summary>
public static class GroupAumid
{
    /// <summary>Stable namespace prefix for all TaskbarFolders group AUMIDs.</summary>
    public const string Prefix = "TaskbarFolders.Group.";

    /// <summary>Builds the AUMID for the supplied group.</summary>
    /// <param name="groupId">Group identifier — usually a GUID hex string.</param>
    public static string For(string groupId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);
        return Prefix + groupId;
    }
}
