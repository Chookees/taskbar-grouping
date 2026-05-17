using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows;
using TaskbarFolders.Launcher.Interop;
using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Launcher.Services;

/// <summary>
/// Default <see cref="ITaskbarPositionHelper"/>. Uses <c>SHAppBarMessage</c> to query the
/// taskbar rectangle + edge and <c>MonitorFromPoint</c> + <c>GetMonitorInfo</c> to resolve
/// the monitor under the cursor (handles multi-monitor and per-monitor taskbars).
/// The pure <see cref="CalculatePlacement"/> method is exposed for unit testing without
/// touching the Win32 surface.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class TaskbarPositionHelper : ITaskbarPositionHelper
{
    /// <summary>Margin between the popup and the taskbar / screen edge, in device-independent pixels.</summary>
    public const double Margin = 8.0;

    /// <inheritdoc/>
    public PopupPlacement ComputePlacement(Size popupSize, PopupPositionPreference preference)
    {
        var taskbar = QueryTaskbar();
        var monitor = QueryMonitorUnderCursor();

        return CalculatePlacement(popupSize, taskbar.rect, taskbar.edge, monitor, preference);
    }

    /// <summary>
    /// Pure positioning logic — picks a top-left so the popup sits adjacent to the supplied
    /// taskbar rectangle on the supplied monitor, then clamps inside the monitor's work area
    /// so a wide popup never falls off-screen on a small display.
    /// </summary>
    /// <param name="popupSize">Popup size in DIPs.</param>
    /// <param name="taskbar">Taskbar rectangle in DIPs.</param>
    /// <param name="edge">Edge the taskbar is anchored to.</param>
    /// <param name="monitorWorkArea">Work area of the monitor under the cursor (excluding the taskbar).</param>
    /// <param name="preference">User vertical-anchor preference.</param>
    public static PopupPlacement CalculatePlacement(
        Size popupSize,
        Rect taskbar,
        TaskbarEdge edge,
        Rect monitorWorkArea,
        PopupPositionPreference preference)
    {
        double left;
        double top;

        // Horizontal anchor: centre on the cursor's X for top/bottom taskbars, hug the
        // taskbar's outer edge for left/right taskbars.
        if (edge is TaskbarEdge.Top or TaskbarEdge.Bottom)
        {
            // Centre under the taskbar by default; preference Above/Below overrides only the vertical side.
            left = taskbar.Left + (taskbar.Width - popupSize.Width) / 2;

            var preferAbove = preference switch
            {
                PopupPositionPreference.Above => true,
                PopupPositionPreference.Below => false,
                _ => edge == TaskbarEdge.Bottom, // Auto: above for bottom taskbar, below for top
            };

            top = preferAbove
                ? taskbar.Top - popupSize.Height - Margin
                : taskbar.Bottom + Margin;
        }
        else
        {
            // Side-anchored taskbar: vertically centre on the monitor work area
            top = monitorWorkArea.Top + (monitorWorkArea.Height - popupSize.Height) / 2;

            left = edge == TaskbarEdge.Left
                ? taskbar.Right + Margin
                : taskbar.Left - popupSize.Width - Margin;
        }

        // Clamp inside the work area so we never end up off-screen on small monitors.
        left = Math.Max(monitorWorkArea.Left, Math.Min(left, monitorWorkArea.Right - popupSize.Width));
        top = Math.Max(monitorWorkArea.Top, Math.Min(top, monitorWorkArea.Bottom - popupSize.Height));

        return new PopupPlacement(left, top);
    }

    private static (Rect rect, TaskbarEdge edge) QueryTaskbar()
    {
        var data = new APPBARDATA
        {
            cbSize = Marshal.SizeOf<APPBARDATA>(),
        };

        NativeMethods.SHAppBarMessage(NativeMethods.ABM_GETTASKBARPOS, ref data);

        var rect = new Rect(data.rc.Left, data.rc.Top, data.rc.Width, data.rc.Height);
        var edge = data.uEdge switch
        {
            NativeMethods.ABE_LEFT => TaskbarEdge.Left,
            NativeMethods.ABE_TOP => TaskbarEdge.Top,
            NativeMethods.ABE_RIGHT => TaskbarEdge.Right,
            _ => TaskbarEdge.Bottom,
        };
        return (rect, edge);
    }

    private static Rect QueryMonitorUnderCursor()
    {
        if (!NativeMethods.GetCursorPos(out var cursor))
        {
            return SystemFallbackWorkArea();
        }

        var hMonitor = NativeMethods.MonitorFromPoint(cursor, NativeMethods.MONITOR_DEFAULTTONEAREST);
        if (hMonitor == IntPtr.Zero)
        {
            return SystemFallbackWorkArea();
        }

        var info = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
        if (!NativeMethods.GetMonitorInfo(hMonitor, ref info))
        {
            return SystemFallbackWorkArea();
        }

        return new Rect(
            info.rcWork.Left,
            info.rcWork.Top,
            info.rcWork.Width,
            info.rcWork.Height);
    }

    private static Rect SystemFallbackWorkArea() => SystemParameters.WorkArea;
}
