using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TaskbarFolders.Core.Interop;

/// <summary>
/// Material to apply behind a window via <c>DwmSetWindowAttribute(DWMWA_SYSTEMBACKDROP_TYPE)</c>.
/// Supported on Windows 11 22H2+; older versions silently keep the classic chrome.
/// </summary>
public enum WindowBackdropKind
{
    /// <summary>Let the system decide.</summary>
    Auto = 0,

    /// <summary>No backdrop material.</summary>
    None = 1,

    /// <summary>Mica — desktop-tinted material for primary app windows.</summary>
    Mica = 2,

    /// <summary>Acrylic — translucent material best suited for transient surfaces (popups, flyouts).</summary>
    Acrylic = 3,

    /// <summary>Mica Alt — slightly more vibrant Mica used by tabbed surfaces in Win11 23H2+.</summary>
    MicaAlt = 4,
}

/// <summary>
/// Thin wrapper around <c>DwmSetWindowAttribute</c> for applying Mica / Acrylic backdrop
/// material. Reused by both Manager (Mica on main window) and Launcher (Acrylic on popup);
/// previously each project had its own duplicate P/Invoke.
/// </summary>
[SupportedOSPlatform("windows")]
public static class WindowBackdrop
{
    private const uint DWMWA_SYSTEMBACKDROP_TYPE = 38;

    /// <summary>
    /// Attempts to apply the specified backdrop material to the supplied window handle.
    /// </summary>
    /// <param name="hwnd">Target window handle (non-zero).</param>
    /// <param name="kind">Backdrop material to apply.</param>
    /// <returns>
    /// <see langword="true"/> if the call succeeded (pre-22H2 Windows return non-zero HRESULTs;
    /// in that case <see langword="false"/> is returned and the caller's themed fallback brush
    /// stays as-is).
    /// </returns>
    public static bool TryApply(IntPtr hwnd, WindowBackdropKind kind)
    {
        if (hwnd == IntPtr.Zero)
        {
            return false;
        }

        var value = (int)kind;
        var hr = DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref value, sizeof(int));
        return hr == 0;
    }

    [DllImport("dwmapi.dll", ExactSpelling = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hWnd, uint attr, ref int attrValue, int attrSize);
}
