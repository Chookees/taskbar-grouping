using System.Windows.Media.Imaging;
using TaskbarFolders.Core.Icons;
using TaskbarFolders.Shared.Utilities;

namespace TaskbarFolders.Manager.Services;

/// <summary>
/// Generates composite icons and launcher configurations for groups.
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
}
