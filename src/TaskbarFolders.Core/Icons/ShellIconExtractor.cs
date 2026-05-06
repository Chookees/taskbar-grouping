using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace TaskbarFolders.Core.Icons;

/// <summary>
/// Extracts icons from executable files, shortcuts, and icon files using the Windows Shell API.
/// </summary>
public sealed class ShellIconExtractor : IIconExtractor
{
    /// <inheritdoc />
    public BitmapSource? ExtractIcon(string filePath, int size = 256)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        string resolvedPath = ResolveShortcut(filePath);

        BitmapSource? icon = ExtractViaExtractIconEx(resolvedPath);
        icon ??= ExtractViaSHGetFileInfo(resolvedPath);
        icon ??= ExtractFromIcoFile(resolvedPath);

        if (icon is not null && (icon.PixelWidth != size || icon.PixelHeight != size))
        {
            icon = ResizeIcon(icon, size);
        }

        return icon;
    }

    private static BitmapSource? ExtractViaExtractIconEx(string filePath)
    {
        var largeIcons = new IntPtr[1];

        int count = NativeMethods.ExtractIconEx(filePath, 0, largeIcons, null, 1);
        if (count <= 0 || largeIcons[0] == IntPtr.Zero)
            return null;

        try
        {
            BitmapSource source = Imaging.CreateBitmapSourceFromHIcon(
                largeIcons[0],
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();
            return source;
        }
        finally
        {
            NativeMethods.DestroyIcon(largeIcons[0]);
        }
    }

    private static BitmapSource? ExtractViaSHGetFileInfo(string filePath)
    {
        var shfi = new NativeMethods.SHFILEINFO();
        uint cbSize = (uint)Marshal.SizeOf(shfi);

        IntPtr result = NativeMethods.SHGetFileInfo(
            filePath, 0, ref shfi, cbSize,
            NativeMethods.SHGFI_ICON | NativeMethods.SHGFI_LARGEICON);

        if (result == IntPtr.Zero || shfi.hIcon == IntPtr.Zero)
            return null;

        try
        {
            BitmapSource source = Imaging.CreateBitmapSourceFromHIcon(
                shfi.hIcon,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();
            return source;
        }
        finally
        {
            NativeMethods.DestroyIcon(shfi.hIcon);
        }
    }

    private static BitmapFrame? ExtractFromIcoFile(string filePath)
    {
        if (!filePath.EndsWith(".ico", StringComparison.OrdinalIgnoreCase) || !File.Exists(filePath))
            return null;

        try
        {
            var decoder = new IconBitmapDecoder(
                new Uri(filePath, UriKind.Absolute),
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);

            BitmapFrame? bestFrame = decoder.Frames
                .OrderByDescending(f => f.PixelWidth)
                .FirstOrDefault();

            bestFrame?.Freeze();
            return bestFrame;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static TransformedBitmap ResizeIcon(BitmapSource source, int size)
    {
        double scaleX = size / (double)source.PixelWidth;
        double scaleY = size / (double)source.PixelHeight;

        var scaled = new TransformedBitmap(source, new System.Windows.Media.ScaleTransform(scaleX, scaleY));
        scaled.Freeze();
        return scaled;
    }

    private static string ResolveShortcut(string path)
    {
        if (!path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
            return path;

        try
        {
            dynamic shell = Activator.CreateInstance(
                Type.GetTypeFromProgID("WScript.Shell")!)!;
            dynamic shortcut = shell.CreateShortcut(path);
            string targetPath = shortcut.TargetPath;
            Marshal.FinalReleaseComObject(shortcut);
            Marshal.FinalReleaseComObject(shell);
            return string.IsNullOrEmpty(targetPath) ? path : targetPath;
        }
        catch (Exception)
        {
            return path;
        }
    }
}
