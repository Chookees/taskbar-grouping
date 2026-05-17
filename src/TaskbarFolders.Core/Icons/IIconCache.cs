using System.Diagnostics.CodeAnalysis;
using System.Windows.Media.Imaging;

namespace TaskbarFolders.Core.Icons;

/// <summary>
/// Caches extracted icons keyed by source path, file modification time, and requested size.
/// Stale entries (older source file) miss naturally because the modification timestamp
/// participates in the key.
/// </summary>
public interface IIconCache
{
    /// <summary>Attempts to read a previously cached icon.</summary>
    /// <param name="sourcePath">Original file path the icon was extracted from.</param>
    /// <param name="size">Pixel size of the cached icon.</param>
    /// <param name="icon">Receives the cached <see cref="BitmapSource"/> on hit; <see langword="null"/> on miss.</param>
    /// <returns><see langword="true"/> if the icon was served from cache; otherwise <see langword="false"/>.</returns>
    bool TryGet(string sourcePath, int size, [NotNullWhen(true)] out BitmapSource? icon);

    /// <summary>Persists an icon under the supplied source-path + size key.</summary>
    /// <param name="sourcePath">Original file path the icon was extracted from.</param>
    /// <param name="size">Pixel size of the icon being cached.</param>
    /// <param name="icon">Icon bitmap. Must be freezable.</param>
    void Set(string sourcePath, int size, BitmapSource icon);

    /// <summary>
    /// Kicks off a background sweep that deletes cache entries past the retention window.
    /// Fire-and-forget — returns immediately. Pre-v0.4 the sweep ran inside the ctor and
    /// blocked startup by ~10-50 ms depending on cache size; v0.4 defers it to post-Show.
    /// Default implementation is a no-op so in-memory or fixture caches do not have to opt in.
    /// </summary>
    void StartBackgroundPrune() { }
}
