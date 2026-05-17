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

    /// <summary>
    /// Sub-folder under the per-user Start Menu where TaskbarFolders publishes anchor .lnk
    /// shortcuts (one per group). Required by Windows.UI.Shell.TaskbarManager:
    /// RequestPinCurrentAppAsync silently fails to persist a pin when no Start Menu entry
    /// with the matching AUMID exists.
    /// </summary>
    public const string StartMenuSubFolderName = "TaskbarFolders";

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
    /// for app data and <see cref="Environment.SpecialFolder.Programs"/> for the per-user
    /// Start Menu (typical production setup).
    /// </summary>
    public AppDataPathProvider()
        : this(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.Programs))
    {
    }

    /// <summary>
    /// Initializes a new instance using a single base directory for BOTH the AppData root
    /// and the Start Menu Programs root. Test-friendly: everything stays under one temp dir.
    /// Production should use the no-arg form (real %APPDATA% + real Start Menu) or the
    /// explicit two-arg form for unusual deployments.
    /// </summary>
    /// <param name="baseDirectory">Directory used for AppData root AND Start Menu Programs base.</param>
    public AppDataPathProvider(string baseDirectory)
        : this(baseDirectory, baseDirectory)
    {
    }

    /// <summary>
    /// Initializes a new instance using the supplied base directories. Two-arg form lets
    /// tests redirect both the AppData root and the Start Menu Programs root to a temp dir.
    /// </summary>
    /// <param name="baseDirectory">Directory under which the <c>TaskbarFolders</c> AppData folder is created.</param>
    /// <param name="startMenuProgramsDirectory">Directory under which the <c>TaskbarFolders</c> Start Menu sub-folder is created.</param>
    public AppDataPathProvider(string baseDirectory, string startMenuProgramsDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(startMenuProgramsDirectory);

        AppDataRoot = Path.Combine(baseDirectory, AppFolderName);
        StartMenuDirectory = Path.Combine(startMenuProgramsDirectory, StartMenuSubFolderName);
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
    public string StartMenuDirectory { get; }

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

    /// <inheritdoc/>
    public string GetStartMenuShortcutFile(string sanitizedFileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sanitizedFileName);
        return Path.Combine(StartMenuDirectory, $"{sanitizedFileName}.lnk");
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
