using System;
using System.Runtime.InteropServices;

namespace TaskbarFolders.Launcher.Interop;

/// <summary>
/// P/Invoke entry points used by the popup positioning helper. Internal to satisfy CA1401
/// (do not expose P/Invoke methods on public types).
/// </summary>
internal static class NativeMethods
{
    // shell32
    [DllImport("shell32.dll", ExactSpelling = true)]
    public static extern IntPtr SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);

    public const uint ABM_GETTASKBARPOS = 0x5;

    public const uint ABE_LEFT = 0;
    public const uint ABE_TOP = 1;
    public const uint ABE_RIGHT = 2;
    public const uint ABE_BOTTOM = 3;

    // user32
    [DllImport("user32.dll", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetMonitorInfoW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    public const uint MONITOR_DEFAULTTONEAREST = 0x2;

    // shcore — per-monitor DPI awareness
    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern bool SetProcessDpiAwarenessContext(IntPtr value);

    // DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = (HANDLE)-4
    public static readonly IntPtr DpiAwarenessContextPerMonitorAwareV2 = new(-4);
}
