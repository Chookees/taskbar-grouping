using System;
using System.IO;

namespace TaskbarFolders.Shared.Configuration;

/// <summary>
/// Default <see cref="IAppDataPathProvider"/> rooted at
/// <see cref="Environment.SpecialFolder.ApplicationData"/>.
/// Tests can use the secondary constructor to point at a temporary directory.
/// </summary>
public sealed class AppDataPathProvider : IAppDataPathProvider
{
    /// <summary>Application sub-folder name under the base directory.</summary>
    public const string AppFolderName = "TaskbarFolders";

    /// <summary>Sub-folder containing per-group configuration files.</summary>
    public const string GroupsFolderName = "groups";

    /// <summary>Sub-folder containing generated icon files.</summary>
    public const string IconsFolderName = "icons";

    /// <summary>Sub-folder containing rotated log files.</summary>
    public const string LogsFolderName = "logs";

    /// <summary>Sub-folder containing per-group pinnable .lnk shortcuts.</summary>
    public const string ShortcutsFolderName = "shortcuts";

    /// <summary>File name of the global settings document.</summary>
    public const string SettingsFileName = "settings.json";

    /// <summary>
    /// Initializes a new instance using <see cref="Environment.SpecialFolder.ApplicationData"/>
    /// as the base directory (typical production setup).
    /// </summary>
    public AppDataPathProvider()
        : this(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))
    {
    }

    /// <summary>
    /// Initializes a new instance using the supplied base directory.
    /// </summary>
    /// <param name="baseDirectory">Directory under which the <c>TaskbarFolders</c> folder is created.</param>
    public AppDataPathProvider(string baseDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseDirectory);

        AppDataRoot = Path.Combine(baseDirectory, AppFolderName);
    }

    /// <inheritdoc/>
    public string AppDataRoot { get; }

    /// <inheritdoc/>
    public string GroupsDirectory => Path.Combine(AppDataRoot, GroupsFolderName);

    /// <inheritdoc/>
    public string IconsDirectory => Path.Combine(AppDataRoot, IconsFolderName);

    /// <inheritdoc/>
    public string LogsDirectory => Path.Combine(AppDataRoot, LogsFolderName);

    /// <inheritdoc/>
    public string ShortcutsDirectory => Path.Combine(AppDataRoot, ShortcutsFolderName);

    /// <inheritdoc/>
    public string SettingsFile => Path.Combine(AppDataRoot, SettingsFileName);

    /// <inheritdoc/>
    public string GetGroupFile(string groupId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);

        return Path.Combine(GroupsDirectory, $"{groupId}.json");
    }

    /// <inheritdoc/>
    public string GetGroupIconFile(string groupId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);

        return Path.Combine(IconsDirectory, $"{groupId}.ico");
    }

    /// <inheritdoc/>
    public string GetGroupShortcutFile(string groupId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);

        return Path.Combine(ShortcutsDirectory, $"{groupId}.lnk");
    }
}
