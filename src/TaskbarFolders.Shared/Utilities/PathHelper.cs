using System.IO;

namespace TaskbarFolders.Shared.Utilities;

/// <summary>
/// Provides standard file system paths for TaskbarFolders.
/// </summary>
public static class PathHelper
{
    private static readonly string AppDataRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TaskbarFolders");

    /// <summary>
    /// Gets the directory where group configuration JSON files are stored.
    /// </summary>
    public static string GroupsDirectory => Path.Combine(AppDataRoot, "groups");

    /// <summary>
    /// Gets the path to the global application settings file.
    /// </summary>
    public static string SettingsFilePath => Path.Combine(AppDataRoot, "settings.json");

    /// <summary>
    /// Gets the directory where generated launcher executables are stored.
    /// </summary>
    public static string LaunchersDirectory => Path.Combine(AppDataRoot, "launchers");

    /// <summary>
    /// Gets the directory where generated composite icons are stored.
    /// </summary>
    public static string IconsDirectory => Path.Combine(AppDataRoot, "icons");

    /// <summary>
    /// Gets the JSON file path for a specific group configuration.
    /// </summary>
    /// <param name="groupId">The unique group identifier.</param>
    public static string GetGroupFilePath(string groupId)
        => Path.Combine(GroupsDirectory, $"{groupId}.json");

    /// <summary>
    /// Gets the generated icon file path for a specific group.
    /// </summary>
    /// <param name="groupId">The unique group identifier.</param>
    public static string GetGroupIconPath(string groupId)
        => Path.Combine(IconsDirectory, $"{groupId}.ico");

    /// <summary>
    /// Gets the shortcut (.lnk) file path for a specific group.
    /// </summary>
    /// <param name="groupId">The unique group identifier.</param>
    /// <param name="groupName">The display name used as the shortcut filename.</param>
    public static string GetGroupShortcutPath(string groupId, string groupName)
    {
        string safeName = SanitizeFileName(groupName);
        return Path.Combine(LaunchersDirectory, $"{safeName}_{groupId[..8]}.lnk");
    }

    /// <summary>
    /// Ensures all required application directories exist.
    /// </summary>
    public static void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(GroupsDirectory);
        Directory.CreateDirectory(LaunchersDirectory);
        Directory.CreateDirectory(IconsDirectory);
    }

    private static string SanitizeFileName(string name)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => Array.IndexOf(invalid, c) >= 0 ? '_' : c));
    }
}
