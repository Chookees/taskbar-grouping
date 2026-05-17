using System;
using System.IO;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TaskbarFolders.Core.Icons;

/// <summary>
/// Default <see cref="IIcoFileWriter"/>. Renders the source bitmap at 16/32/48/256,
/// PNG-encodes each frame, and assembles a Windows ICO container by hand. PNG-in-ICO
/// is supported on Windows Vista+ and produces a noticeably smaller file than the
/// classic BMP-DIB embedding for the 256×256 frame.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class IcoFileWriter : IIcoFileWriter
{
    /// <summary>Frame sizes emitted into the ICO, in the order they appear in the directory.</summary>
    public static readonly int[] FrameSizes = [16, 32, 48, 256];

    private const int IconDirSize = 6;
    private const int IconDirEntrySize = 16;

    /// <inheritdoc/>
    public async Task WriteAsync(BitmapSource source, string targetPath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);

        var bytes = BuildIcoBytes(source);

        var directory = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var temp = targetPath + ".tmp";
        await using (var stream = File.Create(temp))
        {
            await stream.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
        }

        File.Move(temp, targetPath, overwrite: true);
    }

    /// <summary>
    /// Builds the raw ICO byte buffer in memory. Public for tests so they can verify the
    /// header and entry table without writing a file.
    /// </summary>
    /// <param name="source">Source bitmap; any resolution accepted.</param>
    /// <returns>Bytes ready to be written to a <c>.ico</c> file or stream.</returns>
    public static byte[] BuildIcoBytes(BitmapSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var frames = new byte[FrameSizes.Length][];
        for (var i = 0; i < FrameSizes.Length; i++)
        {
            var scaled = ScaleTo(source, FrameSizes[i]);
            frames[i] = EncodeAsPng(scaled);
        }

        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // ICONDIR header (6 bytes).
        writer.Write((ushort)0);                 // reserved
        writer.Write((ushort)1);                 // type = icon
        writer.Write((ushort)FrameSizes.Length); // count

        // Per-entry directory (16 bytes each). Image data follows immediately after the entries.
        var offset = IconDirSize + IconDirEntrySize * FrameSizes.Length;
        for (var i = 0; i < FrameSizes.Length; i++)
        {
            var size = FrameSizes[i];
            var dim = (byte)(size >= 256 ? 0 : size); // 0 means 256 per the ICO spec

            writer.Write(dim);              // width
            writer.Write(dim);              // height
            writer.Write((byte)0);          // color-palette count (0 = no palette)
            writer.Write((byte)0);          // reserved
            writer.Write((ushort)1);        // colour planes
            writer.Write((ushort)32);       // bits per pixel
            writer.Write((uint)frames[i].Length); // bytes
            writer.Write((uint)offset);     // file offset

            offset += frames[i].Length;
        }

        // Image data section.
        foreach (var png in frames)
        {
            writer.Write(png);
        }

        writer.Flush();
        return ms.ToArray();
    }

    private static BitmapSource ScaleTo(BitmapSource source, int targetSize)
    {
        if (source.PixelWidth == targetSize && source.PixelHeight == targetSize)
        {
            // Avoid pointless re-render at exact size.
            return source;
        }

        var visual = new DrawingVisual();
        RenderOptions.SetBitmapScalingMode(visual, BitmapScalingMode.HighQuality);

        using (var ctx = visual.RenderOpen())
        {
            ctx.DrawImage(source, new Rect(0, 0, targetSize, targetSize));
        }

        var bitmap = new RenderTargetBitmap(targetSize, targetSize, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
    }

    private static byte[] EncodeAsPng(BitmapSource bitmap)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));

        using var ms = new MemoryStream();
        encoder.Save(ms);
        return ms.ToArray();
    }
}
