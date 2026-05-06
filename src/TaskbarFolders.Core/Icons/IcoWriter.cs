using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TaskbarFolders.Core.Icons;

/// <summary>
/// Writes BitmapSource images as multi-resolution .ico files.
/// </summary>
public static class IcoWriter
{
    private static readonly int[] DefaultSizes = [16, 32, 48, 256];

    /// <summary>
    /// Writes a BitmapSource as a multi-resolution .ico file.
    /// </summary>
    /// <param name="source">The source image (should be at least 256x256).</param>
    /// <param name="outputPath">The output .ico file path.</param>
    public static void Write(BitmapSource source, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        string? directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var pngEntries = CreatePngEntries(source);

        using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        WriteIcoFile(stream, pngEntries);
    }

    /// <summary>
    /// Writes a BitmapSource as a multi-resolution .ico file to a stream.
    /// </summary>
    /// <param name="source">The source image.</param>
    /// <param name="outputStream">The output stream.</param>
    public static void Write(BitmapSource source, Stream outputStream)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(outputStream);

        var pngEntries = CreatePngEntries(source);
        WriteIcoFile(outputStream, pngEntries);
    }

    private static List<byte[]> CreatePngEntries(BitmapSource source)
    {
        var pngEntries = new List<byte[]>();
        foreach (int size in DefaultSizes)
        {
            BitmapSource resized = Resize(source, size);
            pngEntries.Add(EncodePng(resized));
        }
        return pngEntries;
    }

    private static void WriteIcoFile(Stream stream, List<byte[]> pngEntries)
    {
        using var writer = new BinaryWriter(stream, System.Text.Encoding.Default, leaveOpen: true);

        // ICO header
        writer.Write((ushort)0);                      // reserved
        writer.Write((ushort)1);                      // type: icon
        writer.Write((ushort)pngEntries.Count);       // number of images

        int headerSize = 6;
        int directorySize = 16 * pngEntries.Count;
        int dataOffset = headerSize + directorySize;

        // Directory entries
        for (int i = 0; i < pngEntries.Count; i++)
        {
            int size = DefaultSizes[i];
            byte widthByte = size >= 256 ? (byte)0 : (byte)size;
            byte heightByte = size >= 256 ? (byte)0 : (byte)size;

            writer.Write(widthByte);                  // width
            writer.Write(heightByte);                 // height
            writer.Write((byte)0);                    // color palette
            writer.Write((byte)0);                    // reserved
            writer.Write((ushort)1);                  // color planes
            writer.Write((ushort)32);                 // bits per pixel
            writer.Write(pngEntries[i].Length);        // data size
            writer.Write(dataOffset);                 // data offset

            dataOffset += pngEntries[i].Length;
        }

        // Image data
        foreach (byte[] png in pngEntries)
        {
            writer.Write(png);
        }
    }

    private static BitmapSource Resize(BitmapSource source, int size)
    {
        if (source.PixelWidth == size && source.PixelHeight == size)
            return source;

        double scaleX = size / (double)source.PixelWidth;
        double scaleY = size / (double)source.PixelHeight;

        var scaled = new TransformedBitmap(source, new ScaleTransform(scaleX, scaleY));
        scaled.Freeze();
        return scaled;
    }

    private static byte[] EncodePng(BitmapSource source)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(source));

        using var ms = new MemoryStream();
        encoder.Save(ms);
        return ms.ToArray();
    }
}
