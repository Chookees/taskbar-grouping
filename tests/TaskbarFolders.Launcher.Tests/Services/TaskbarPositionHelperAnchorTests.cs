using System.Windows;
using FluentAssertions;
using TaskbarFolders.Launcher.Services;
using TaskbarFolders.Shared.Models;
using Xunit;

namespace TaskbarFolders.Launcher.Tests.Services;

/// <summary>
/// Anchor-specific assertions for <see cref="TaskbarPositionHelper.CalculatePlacement"/>:
/// proves that the popup is centred on the supplied <c>clickAnchor</c> instead of the
/// taskbar centre (the v0.2 bug), and that the clamp guards still apply when the click
/// lands near a monitor edge.
/// </summary>
public class TaskbarPositionHelperAnchorTests
{
    private static readonly Rect _monitorWorkAreaBottomBar = new(0, 0, 1920, 1040);
    private static readonly Rect _bottomTaskbar = new(0, 1040, 1920, 40);

    [Fact]
    public void BottomTaskbar_ClickInTaskbarCentre_PopupCentredOverClick()
    {
        // Baseline parity with the v0.2 algorithm: when the click lands at the taskbar
        // centre the popup ends up exactly where the old "centre on taskbar" code put it.
        var anchor = new Point(960, 1060); // dead centre of a 1920-wide bottom taskbar
        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(400, 300), _bottomTaskbar, TaskbarEdge.Bottom, _monitorWorkAreaBottomBar, PopupPositionPreference.Auto, anchor);

        placement.Left.Should().Be(960 - 200);
    }

    [Fact]
    public void BottomTaskbar_ClickAtFarLeft_PopupClampsToWorkAreaLeft()
    {
        // Pinned tile at the very left of the taskbar: anchor.X - width/2 would be negative;
        // clamp must pin the popup to the monitor's left edge.
        var anchor = new Point(20, 1060);
        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(400, 300), _bottomTaskbar, TaskbarEdge.Bottom, _monitorWorkAreaBottomBar, PopupPositionPreference.Auto, anchor);

        placement.Left.Should().Be(_monitorWorkAreaBottomBar.Left);
    }

    [Fact]
    public void BottomTaskbar_ClickAtFarRight_PopupClampsToWorkAreaRight()
    {
        var anchor = new Point(1900, 1060);
        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(400, 300), _bottomTaskbar, TaskbarEdge.Bottom, _monitorWorkAreaBottomBar, PopupPositionPreference.Auto, anchor);

        placement.Left.Should().Be(_monitorWorkAreaBottomBar.Right - 400);
    }

    [Fact]
    public void BottomTaskbar_ClickOnSecondaryMonitorWithNegativeX_PopupCentresOnClick()
    {
        // Secondary monitor on the user's left, primary at (0,0). Anchor must drive
        // placement on the correct monitor — proves QueryMonitor uses the anchor, not the
        // primary monitor's origin.
        var leftWork = new Rect(-1920, 0, 1920, 1040);
        var leftBar = new Rect(-1920, 1040, 1920, 40);
        var anchor = new Point(-960, 1060);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(400, 300), leftBar, TaskbarEdge.Bottom, leftWork, PopupPositionPreference.Auto, anchor);

        placement.Left.Should().Be(-960 - 200);
    }

    [Fact]
    public void LeftTaskbar_ClickHigh_PopupVerticallyAnchoredOnClick()
    {
        var leftBar = new Rect(0, 0, 60, 1080);
        var workArea = new Rect(60, 0, 1860, 1080);
        var anchor = new Point(30, 200);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(400, 300), leftBar, TaskbarEdge.Left, workArea, PopupPositionPreference.Auto, anchor);

        placement.Top.Should().Be(200 - 150);
        placement.Left.Should().Be(60 + TaskbarPositionHelper.Margin);
    }

    [Fact]
    public void LeftTaskbar_ClickAtBottom_PopupClampsToWorkAreaBottom()
    {
        var leftBar = new Rect(0, 0, 60, 1080);
        var workArea = new Rect(60, 0, 1860, 1080);
        var anchor = new Point(30, 1070);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(400, 300), leftBar, TaskbarEdge.Left, workArea, PopupPositionPreference.Auto, anchor);

        placement.Top.Should().Be(workArea.Bottom - 300);
    }
}
