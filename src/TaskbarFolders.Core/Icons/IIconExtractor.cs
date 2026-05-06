using System.Windows.Media.Imaging;

namespace TaskbarFolders.Core.Icons;

/// <summary>
/// Extracts icons from executable files, shortcuts, and icon files.
/// </summary>
public interface IIconExtractor
{
    /// <summary>
    /// Extracts the icon from the specified file path.
    /// </summary>
    /// <param name="filePath">Path to an .exe, .lnk, or .ico file.</param>
    /// <param name="size">Desired icon size in pixels.</param>
    /// <returns>The extracted icon as a BitmapSource, or null if extraction fails.</returns>
    BitmapSource? ExtractIcon(string filePath, int size = 256);
}
