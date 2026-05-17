namespace TaskbarFolders.Shared.Configuration;

/// <summary>
/// Provides absolute paths under <c>%APPDATA%/TaskbarFolders</c> where group configurations,
/// settings, and generated icons are persisted.
/// </summary>
public interface IAppDataPathProvider
{
    /// <summary>Gets the root directory used by TaskbarFolders for user-scoped data.</summary>
    string AppDataRoot { get; }

    /// <summary>Gets the directory containing per-group JSON configuration files.</summary>
    string GroupsDirectory { get; }

    /// <summary>Gets the directory containing generated composite and per-group icons.</summary>
    string IconsDirectory { get; }

    /// <summary>Gets the directory containing rotated diagnostic log files.</summary>
    string LogsDirectory { get; }

    /// <summary>Gets the directory containing per-group pinnable shortcut (.lnk) files.</summary>
    string ShortcutsDirectory { get; }

    /// <summary>Gets the path to the global application settings file.</summary>
    string SettingsFile { get; }

    /// <summary>Builds the full path for a single group's JSON configuration file.</summary>
    /// <param name="groupId">Group identifier.</param>
    string GetGroupFile(string groupId);

    /// <summary>Builds the full path for a single group's composite icon (.ico) file.</summary>
    /// <param name="groupId">Group identifier.</param>
    string GetGroupIconFile(string groupId);

    /// <summary>Builds the full path for a single group's pinnable shortcut (.lnk) file.</summary>
    /// <param name="groupId">Group identifier — used as the file name stem so distinct groups never collide regardless of display name.</param>
    string GetGroupShortcutFile(string groupId);
}
