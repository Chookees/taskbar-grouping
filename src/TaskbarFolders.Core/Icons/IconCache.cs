using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media.Imaging;

namespace TaskbarFolders.Core.Icons;

/// <summary>
/// Caches extracted and generated icons in memory and on disk.
/// </summary>
public sealed class IconCache
{
    private readonly ConcurrentDictionary<string, BitmapSource> _memoryCache = new();
    private readonly string _diskCachePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="IconCache"/> class.
    /// </summary>
    /// <param name="diskCachePath">Path to the disk cache directory. If null, uses a default location.</param>
    public IconCache(string? diskCachePath = null)
    {
        _diskCachePath = diskCachePath
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TaskbarFolders", "cache", "icons");

        Directory.CreateDirectory(_diskCachePath);
    }

    /// <summary>
    /// Gets a cached icon or extracts it using the provided factory.
    /// </summary>
    /// <param name="key">Cache key (typically the source file path).</param>
    /// <param name="factory">Factory function to produce the icon if not cached.</param>
    /// <returns>The cached or newly created icon, or null if the factory returns null.</returns>
    public BitmapSource? GetOrCreate(string key, Func<BitmapSource?> factory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        string normalizedKey = NormalizeKey(key);

        if (_memoryCache.TryGetValue(normalizedKey, out BitmapSource? cached))
            return cached;

        BitmapImage? fromDisk = LoadFromDisk(normalizedKey);
        if (fromDisk is not null)
        {
            _memoryCache.TryAdd(normalizedKey, fromDisk);
            return fromDisk;
        }

        BitmapSource? created = factory();
        if (created is null)
            return null;

        created.Freeze();
        _memoryCache.TryAdd(normalizedKey, created);
        SaveToDisk(normalizedKey, created);

        return created;
    }

    /// <summary>
    /// Removes a cached icon.
    /// </summary>
    /// <param name="key">Cache key to invalidate.</param>
    public void Invalidate(string key)
    {
        string normalizedKey = NormalizeKey(key);
        _memoryCache.TryRemove(normalizedKey, out _);

        string filePath = GetDiskPath(normalizedKey);
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    /// <summary>
    /// Clears the entire cache (memory and disk).
    /// </summary>
    public void Clear()
    {
        _memoryCache.Clear();

        if (Directory.Exists(_diskCachePath))
        {
            foreach (string file in Directory.GetFiles(_diskCachePath, "*.png"))
            {
                File.Delete(file);
            }
        }
    }

    private static BitmapImage? LoadFromDisk(string normalizedKey)
    {
        string filePath = GetDiskPath(normalizedKey);
        if (!File.Exists(filePath))
            return null;

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static void SaveToDisk(string normalizedKey, BitmapSource source)
    {
        string filePath = GetDiskPath(normalizedKey);

        try
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));

            using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            encoder.Save(stream);
        }
        catch (Exception)
        {
            // disk cache is best-effort
        }
    }

    private static string GetDiskPath(string normalizedKey)
    {
        string basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TaskbarFolders", "cache", "icons");
        return Path.Combine(basePath, normalizedKey + ".png");
    }

    private static string NormalizeKey(string key)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(key.ToUpperInvariant()));
        return Convert.ToHexString(hash)[..16];
    }
}
