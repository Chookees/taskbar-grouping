using System.IO;
using System.Windows.Media.Imaging;
using TaskbarFolders.Core.Icons;
using TaskbarFolders.Shared.Utilities;

namespace TaskbarFolders.Manager.Services;

/// <summary>
/// Generates composite icons, shortcuts, and launcher configurations for groups.
/// </summary>
public sealed class LauncherGenerator
{
    /// <summary>
    /// Generates and saves a composite .ico file for the specified group.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Instance method for DI testability")]
    public void GenerateGroupIcon(string groupId, BitmapSource compositeIcon)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);
        ArgumentNullException.ThrowIfNull(compositeIcon);

        PathHelper.EnsureDirectoriesExist();
        string iconPath = PathHelper.GetGroupIconPath(groupId);
        IcoWriter.Write(compositeIcon, iconPath);
    }

    /// <summary>
    /// Creates a .lnk shortcut for the group that launches the popup.
    /// </summary>
    /// <returns>The path to the created shortcut, or null if the launcher exe was not found.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Instance method for DI testability")]
    public string? GenerateShortcut(string groupId, string groupName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);

        string? launcherPath = FindLauncherExe();
        if (launcherPath is null)
            return null;

        PathHelper.EnsureDirectoriesExist();
        string iconPath = PathHelper.GetGroupIconPath(groupId);
        string shortcutPath = PathHelper.GetGroupShortcutPath(groupId, groupName);

        RemoveExistingShortcuts(groupId);
        CreateShortcut(shortcutPath, launcherPath, $"--group-id {groupId}", iconPath, groupName);

        return shortcutPath;
    }

    /// <summary>
    /// Deletes all generated files (icon, shortcuts) for a group.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Instance method for DI testability")]
    public void DeleteGroupFiles(string groupId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);

        string iconPath = PathHelper.GetGroupIconPath(groupId);
        if (File.Exists(iconPath))
            File.Delete(iconPath);

        RemoveExistingShortcuts(groupId);
    }

    private static string? FindLauncherExe()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        const string exeName = "TaskbarFolders.Launcher.exe";

        // Installed layout: both exes in the same directory
        string sameDirPath = Path.Combine(baseDir, exeName);
        if (File.Exists(sameDirPath))
            return sameDirPath;

        string? parentDir = Path.GetDirectoryName(baseDir.TrimEnd(Path.DirectorySeparatorChar));
        if (parentDir is null)
            return null;

        // Old installer layout: {app}\Launcher\
        string launcherSubdir = Path.Combine(parentDir, "Launcher", exeName);
        if (File.Exists(launcherSubdir))
            return launcherSubdir;

        // Development layout: sibling project output (e.g. bin/Release/net10.0-windows/)
        string siblingProject = Path.Combine(parentDir, "TaskbarFolders.Launcher", exeName);
        if (File.Exists(siblingProject))
            return siblingProject;

        // Development layout: parallel project under same bin/Release/
        string? grandParent = Path.GetDirectoryName(parentDir);
        if (grandParent is not null)
        {
            string parallelBin = Path.Combine(grandParent, "TaskbarFolders.Launcher", "bin", "Release");
            if (Directory.Exists(parallelBin))
            {
                string[] candidates = Directory.GetFiles(parallelBin, exeName, SearchOption.AllDirectories);
                if (candidates.Length > 0)
                    return candidates[0];
            }
        }

        return null;
    }

    private static void CreateShortcut(string shortcutPath, string targetPath, string arguments, string iconPath, string description)
    {
        Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
        if (shellType is null)
            return;

        dynamic shell = Activator.CreateInstance(shellType)!;
        try
        {
            dynamic shortcut = shell.CreateShortcut(shortcutPath);
            try
            {
                shortcut.TargetPath = targetPath;
                shortcut.Arguments = arguments;
                shortcut.Description = description;
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);

                if (File.Exists(iconPath))
                    shortcut.IconLocation = $"{iconPath},0";

                shortcut.Save();
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(shortcut);
            }
        }
        finally
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(shell);
        }
    }

    private static void RemoveExistingShortcuts(string groupId)
    {
        string shortIdSuffix = $"_{groupId[..8]}.lnk";
        string launchersDir = PathHelper.LaunchersDirectory;

        if (!Directory.Exists(launchersDir))
            return;

        foreach (string file in Directory.GetFiles(launchersDir, "*.lnk"))
        {
            if (Path.GetFileName(file).EndsWith(shortIdSuffix, StringComparison.OrdinalIgnoreCase))
                File.Delete(file);
        }
    }
}
