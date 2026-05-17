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
/// the monitor under the click point (handles multi-monitor and per-monitor taskbars).
/// The pure <see cref="CalculatePlacement"/> method is exposed for unit testing without
/// touching the Win32 surface.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class TaskbarPositionHelper : ITaskbarPositionHelper
{
    /// <summary>Margin between the popup and the taskbar / screen edge, in device-independent pixels.</summary>
    public const double Margin = 8.0;

    private readonly ICursorAnchor _cursorAnchor;

    /// <summary>Initializes a new instance.</summary>
    /// <param name="cursorAnchor">Seeded cursor anchor — see <see cref="ICursorAnchor"/>.</param>
    public TaskbarPositionHelper(ICursorAnchor cursorAnchor)
    {
        ArgumentNullException.ThrowIfNull(cursorAnchor);
        _cursorAnchor = cursorAnchor;
    }

    /// <inheritdoc/>
    public PopupPlacement ComputePlacement(Size popupSize, PopupPositionPreference preference)
    {
        var anchor = _cursorAnchor.Position;
        var taskbar = QueryTaskbar();
        var monitor = QueryMonitor(anchor);

        return CalculatePlacement(popupSize, taskbar.rect, taskbar.edge, monitor, preference, anchor);
    }

    /// <summary>
    /// Pure positioning logic. The popup is anchored on <paramref name="clickAnchor"/> —
    /// horizontally for top/bottom taskbars, vertically for left/right taskbars — then
    /// hugs the taskbar's outer edge on the remaining axis, then clamps inside the
    /// monitor's work area so a wide popup never falls off-screen on a small display.
    /// </summary>
    /// <param name="popupSize">Popup size in DIPs.</param>
    /// <param name="taskbar">Taskbar rectangle in DIPs.</param>
    /// <param name="edge">Edge the taskbar is anchored to.</param>
    /// <param name="monitorWorkArea">Work area of the monitor (excluding the taskbar).</param>
    /// <param name="preference">User vertical-anchor preference.</param>
    /// <param name="clickAnchor">Cursor position at tile-click time — see <see cref="ICursorAnchor"/>.</param>
    public static PopupPlacement CalculatePlacement(
        Size popupSize,
        Rect taskbar,
        TaskbarEdge edge,
        Rect monitorWorkArea,
        PopupPositionPreference preference,
        Point clickAnchor)
    {
        double left;
        double top;

        if (edge is TaskbarEdge.Top or TaskbarEdge.Bottom)
        {
            // Centre the popup on the cursor X so it sits directly over the clicked tile.
            // Pre-v0.3 used taskbar-centre — produced "random" placement for tiles far from
            // the taskbar middle.
            left = clickAnchor.X - popupSize.Width / 2;

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
            // Side taskbar: vertically anchor on cursor Y (same fix as above, just rotated).
            top = clickAnchor.Y - popupSize.Height / 2;

            left = edge == TaskbarEdge.Left
                ? taskbar.Right + Margin
                : taskbar.Left - popupSize.Width - Margin;
        }

        // Clamp inside the work area so we never end up off-screen on small monitors or
        // when the click landed near a screen edge.
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

    private static Rect QueryMonitor(Point anchor)
    {
        var pt = new POINT { X = (int)anchor.X, Y = (int)anchor.Y };
        var hMonitor = NativeMethods.MonitorFromPoint(pt, NativeMethods.MONITOR_DEFAULTTONEAREST);
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
