using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TaskbarFolders.Core.Icons;

/// <summary>
/// Writes a multi-resolution Windows <c>.ico</c> file from a single source bitmap.
/// </summary>
public interface IIcoFileWriter
{
    /// <summary>
    /// Encodes <paramref name="source"/> into a 16/32/48/256 multi-resolution ICO and writes
    /// it atomically to <paramref name="targetPath"/>.
    /// </summary>
    /// <param name="source">Source bitmap. A 256×256 input gives the best downscale quality.</param>
    /// <param name="targetPath">Absolute path of the destination <c>.ico</c> file. Existing files are replaced.</param>
    /// <param name="cancellationToken">Cancellation token honoured around the file write.</param>
    Task WriteAsync(BitmapSource source, string targetPath, CancellationToken cancellationToken = default);
}
