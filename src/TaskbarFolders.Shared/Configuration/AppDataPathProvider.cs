using System;
using System.IO;
using System.Text.RegularExpressions;

namespace TaskbarFolders.Shared.Configuration;

/// <summary>
/// Default <see cref="IAppDataPathProvider"/> rooted at
/// <see cref="Environment.SpecialFolder.ApplicationData"/>.
/// Tests can use the secondary constructor to point at a temporary directory.
/// </summary>
public sealed partial class AppDataPathProvider : IAppDataPathProvider
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
    /// Whitelist for group ids. Permits the characters Windows AUMIDs accept
    /// (<c>A-Z a-z 0-9 . _ -</c>) — also rejects any path-separator or <c>..</c>
    /// sequence so a hand-edited JSON cannot escape the per-app data root.
    /// Length cap is 96 — the AUMID hard limit is 128 chars, the <c>"TaskbarFolders.Group."</c>
    /// prefix is 21, so 96 leaves 11 chars of headroom (room for the future to lengthen the
    /// prefix or add a discriminator without re-validating every persisted id).
    /// </summary>
    [GeneratedRegex(@"^[A-Za-z0-9._-]{1,96}$", RegexOptions.CultureInvariant)]
    private static partial Regex GroupIdPattern();

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
        ValidateGroupId(groupId);
        return Path.Combine(GroupsDirectory, $"{groupId}.json");
    }

    /// <inheritdoc/>
    public string GetGroupIconFile(string groupId)
    {
        ValidateGroupId(groupId);
        return Path.Combine(IconsDirectory, $"{groupId}.ico");
    }

    /// <inheritdoc/>
    public string GetGroupShortcutFile(string groupId)
    {
        ValidateGroupId(groupId);
        return Path.Combine(ShortcutsDirectory, $"{groupId}.lnk");
    }

    private static void ValidateGroupId(string groupId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);

        if (!GroupIdPattern().IsMatch(groupId))
        {
            throw new ArgumentException(
                $"Invalid group id '{groupId}'. Must match {GroupIdPattern()} — letters, digits, dot, underscore, hyphen; max 96 chars; no path separators.",
                nameof(groupId));
        }
    }
}
