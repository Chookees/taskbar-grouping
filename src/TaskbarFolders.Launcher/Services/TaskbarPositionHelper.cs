using System.Runtime.InteropServices;
using System.Windows;

namespace TaskbarFolders.Launcher.Services;

/// <summary>
/// Determines the optimal popup position relative to the Windows taskbar.
/// </summary>
public static partial class TaskbarPositionHelper
{
    private const int ABM_GETTASKBARPOS = 0x00000005;

    /// <summary>
    /// Calculates the popup window position near the taskbar.
    /// </summary>
    /// <param name="popupWidth">Width of the popup window.</param>
    /// <param name="popupHeight">Height of the popup window.</param>
    /// <returns>The calculated position for the popup window.</returns>
    public static Point GetPopupPosition(double popupWidth, double popupHeight)
    {
        var appBarData = new APPBARDATA { cbSize = (uint)Marshal.SizeOf<APPBARDATA>() };
        SHAppBarMessage(ABM_GETTASKBARPOS, ref appBarData);

        Rect taskbarRect = new(
            appBarData.rc.left,
            appBarData.rc.top,
            appBarData.rc.right - appBarData.rc.left,
            appBarData.rc.bottom - appBarData.rc.top);

        Rect workArea = SystemParameters.WorkArea;
        Point cursorPos = GetCursorPosition();

        double x = cursorPos.X - (popupWidth / 2);
        double y;

        if (taskbarRect.Top <= 0 && taskbarRect.Height < workArea.Height / 2)
        {
            // Taskbar at top
            y = taskbarRect.Bottom + 8;
        }
        else if (taskbarRect.Left <= 0 && taskbarRect.Width < workArea.Width / 2)
        {
            // Taskbar at left
            x = taskbarRect.Right + 8;
            y = cursorPos.Y - (popupHeight / 2);
        }
        else if (taskbarRect.Left >= workArea.Width)
        {
            // Taskbar at right
            x = taskbarRect.Left - popupWidth - 8;
            y = cursorPos.Y - (popupHeight / 2);
        }
        else
        {
            // Taskbar at bottom (default)
            y = taskbarRect.Top - popupHeight - 8;
        }

        x = Math.Max(0, Math.Min(x, workArea.Width - popupWidth));
        y = Math.Max(0, Math.Min(y, workArea.Height - popupHeight));

        return new Point(x, y);
    }

    private static Point GetCursorPosition()
    {
        GetCursorPos(out POINT point);
        return new Point(point.X, point.Y);
    }

    [LibraryImport("shell32.dll")]
    private static partial IntPtr SHAppBarMessage(int dwMessage, ref APPBARDATA pData);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct APPBARDATA
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public uint uEdge;
        public RECT rc;
        public int lParam;
    }
}
