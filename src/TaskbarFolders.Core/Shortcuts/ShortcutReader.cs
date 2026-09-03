using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using TaskbarFolders.Core.Interop;

namespace TaskbarFolders.Core.Shortcuts;

/// <summary>
/// Default <see cref="IShortcutReader"/>. Loads a <c>.lnk</c> through <see cref="IPersistFile"/>
/// and reads <c>PKEY_AppUserModel_ID</c> off the same <see cref="IPropertyStore"/> that
/// <see cref="ShortcutGenerator"/> writes it to.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class ShortcutReader : IShortcutReader
{
    /// <summary>Open the storage read-only; we never write through this path.</summary>
    private const uint StgmRead = 0x00000000;

    private readonly ILogger<ShortcutReader>? _logger;

    /// <summary>Initializes a new instance.</summary>
    /// <param name="logger">Optional logger.</param>
    public ShortcutReader(ILogger<ShortcutReader>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public string? TryReadAumid(string shortcutPath)
    {
        if (string.IsNullOrWhiteSpace(shortcutPath) || !File.Exists(shortcutPath))
        {
            return null;
        }

        // Instantiate outside the try so a CLSID-not-registered failure cannot leave the
        // finally releasing a half-constructed RCW (same discipline as ShellIconExtractor).
        var link = new ShellLink();
        try
        {
            ((IPersistFile)link).Load(shortcutPath, StgmRead);

            var store = (IPropertyStore)link;
            var key = PropertyKeys.AppUserModelId;
            store.GetValue(ref key, out var variant);

            try
            {
                return variant.vt == PropVariantType.VT_LPWSTR && variant.pwszVal != IntPtr.Zero
                    ? Marshal.PtrToStringUni(variant.pwszVal)
                    : null;
            }
            finally
            {
                // GetValue allocated the string on our behalf.
                _ = PropVariantNativeMethods.PropVariantClear(ref variant);
            }
        }
        catch (Exception ex) when (ex is COMException or InvalidCastException or IOException or UnauthorizedAccessException)
        {
            _logger?.LogDebug(ex, "Could not read the AppUserModelID of {Path}.", shortcutPath);
            return null;
        }
        finally
        {
            Marshal.FinalReleaseComObject(link);
        }
    }
}
