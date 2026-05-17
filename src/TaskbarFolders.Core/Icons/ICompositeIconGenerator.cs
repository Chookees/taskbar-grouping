using System.Windows.Media.Imaging;

namespace TaskbarFolders.Core.Icons;

/// <summary>
/// Generates composite icons from multiple source icons in a grid layout.
/// </summary>
public interface ICompositeIconGenerator
{
    /// <summary>
    /// Creates a 2x2 composite icon from the provided source icons.
    /// </summary>
    /// <param name="icons">Source icons to compose (up to 4).</param>
    /// <param name="outputSize">Output icon size in pixels.</param>
    /// <returns>The composite icon as a BitmapSource.</returns>
    BitmapSource GenerateComposite(IReadOnlyList<BitmapSource> icons, int outputSize = 256);
}
