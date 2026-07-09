using System.Windows;
using FluentAssertions;
using TaskbarFolders.Launcher.Services;
using TaskbarFolders.Shared.Models;
using Xunit;

namespace TaskbarFolders.Launcher.Tests.Services;

/// <summary>
/// Edge-probing + clamping assertions for <see cref="TaskbarPositionHelper.CalculatePlacement"/>.
/// Each case fixes the click anchor at the centre of its respective monitor work area so the
/// horizontal/vertical anchor math is a no-op and only edge logic + clamping is asserted.
/// Anchor-specific behaviour (clickAnchor near monitor edges) lives in
/// <see cref="TaskbarPositionHelperAnchorTests"/>.
/// </summary>
public class TaskbarPositionHelperTests
{
    // 1920×1080 primary monitor; bottom taskbar 40px tall.
    private static readonly Rect _monitorWorkAreaBottomBar = new(0, 0, 1920, 1040);
    private static readonly Rect _bottomTaskbar = new(0, 1040, 1920, 40);

    private static Point Centre(Rect workArea) =>
        new(workArea.Left + workArea.Width / 2, workArea.Top + workArea.Height / 2);

    [Fact]
    public void BottomTaskbar_Auto_PlacesPopupAbove_AndHorizontallyCentred()
    {
        var size = new Size(400, 300);
        var anchor = Centre(_monitorWorkAreaBottomBar);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            size, _bottomTaskbar, TaskbarEdge.Bottom, _monitorWorkAreaBottomBar, PopupPositionPreference.Auto, anchor);

        // Above the taskbar, with the margin
        placement.Top.Should().Be(1040 - 300 - TaskbarPositionHelper.Margin);
        // Centred on the click anchor (which happens to be monitor centre here)
        placement.Left.Should().Be(anchor.X - 400 / 2);
    }

    [Fact]
    public void TopTaskbar_Auto_PlacesPopupBelow()
    {
        var topTaskbar = new Rect(0, 0, 1920, 40);
        var workArea = new Rect(0, 40, 1920, 1040);
        var anchor = Centre(workArea);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(400, 300), topTaskbar, TaskbarEdge.Top, workArea, PopupPositionPreference.Auto, anchor);

        placement.Top.Should().Be(40 + TaskbarPositionHelper.Margin);
    }

    [Fact]
    public void BottomTaskbar_BelowPreference_OverridesAutoAndPlacesPopupBelowTaskbar()
    {
        // Bottom-taskbar Auto puts the popup above. Preference=Below forces it below
        // (unusual but documented). Should be clamped to the work area.
        var anchor = Centre(_monitorWorkAreaBottomBar);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(400, 300), _bottomTaskbar, TaskbarEdge.Bottom, _monitorWorkAreaBottomBar, PopupPositionPreference.Below, anchor);

        // Top should be at taskbar.Bottom + margin = 1080 + 8 = 1088 → clamped to fit
        placement.Top.Should().BeLessOrEqualTo(_monitorWorkAreaBottomBar.Bottom - 300);
    }

    [Fact]
    public void LeftTaskbar_PlacesPopupToTheRight_AndVerticallyAnchoredOnClick()
    {
        var leftTaskbar = new Rect(0, 0, 60, 1080);
        var workArea = new Rect(60, 0, 1860, 1080);
        var anchor = Centre(workArea);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(400, 300), leftTaskbar, TaskbarEdge.Left, workArea, PopupPositionPreference.Auto, anchor);

        placement.Left.Should().Be(60 + TaskbarPositionHelper.Margin);
        placement.Top.Should().Be(anchor.Y - 300 / 2);
    }

    [Fact]
    public void RightTaskbar_PlacesPopupToTheLeft_AndVerticallyAnchoredOnClick()
    {
        var rightTaskbar = new Rect(1860, 0, 60, 1080);
        var workArea = new Rect(0, 0, 1860, 1080);
        var anchor = Centre(workArea);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(400, 300), rightTaskbar, TaskbarEdge.Right, workArea, PopupPositionPreference.Auto, anchor);

        placement.Left.Should().Be(1860 - 400 - TaskbarPositionHelper.Margin);
        placement.Top.Should().Be(anchor.Y - 300 / 2);
    }

    [Fact]
    public void Placement_IsClampedToWorkArea_WhenPopupExceedsMonitorWidth()
    {
        // Tiny monitor (800x600) with a wide popup (1000) — left should clamp to 0.
        var smallWorkArea = new Rect(0, 0, 800, 560);
        var bottomBar = new Rect(0, 560, 800, 40);
        var anchor = Centre(smallWorkArea);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(1000, 300), bottomBar, TaskbarEdge.Bottom, smallWorkArea, PopupPositionPreference.Auto, anchor);

        placement.Left.Should().Be(0, "popup wider than monitor → clamp to work-area left");
    }

    [Fact]
    public void Placement_OnSecondaryMonitor_UsesItsWorkArea_NotPrimary()
    {
        // Secondary monitor at x=1920, 1920x1080, with bottom taskbar
        var secondaryWork = new Rect(1920, 0, 1920, 1040);
        var secondaryBar = new Rect(1920, 1040, 1920, 40);
        var anchor = Centre(secondaryWork);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(400, 300), secondaryBar, TaskbarEdge.Bottom, secondaryWork, PopupPositionPreference.Auto, anchor);

        placement.Left.Should().Be(anchor.X - 400 / 2);
        placement.Top.Should().Be(1040 - 300 - TaskbarPositionHelper.Margin);
    }

    [Fact]
    public void Placement_ClampsTop_WhenPopupTallerThanWorkArea()
    {
        // Pathological: popup taller than the work area. Top should clamp to work-area top
        // so the header is still visible (worse than scrolling but better than off-screen).
        var workArea = new Rect(0, 0, 1920, 400);
        var bottomBar = new Rect(0, 400, 1920, 40);
        var anchor = Centre(workArea);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(400, 700), bottomBar, TaskbarEdge.Bottom, workArea, PopupPositionPreference.Auto, anchor);

        placement.Top.Should().Be(0, "popup taller than work area → clamp to work-area top");
    }

    [Fact]
    public void Placement_OnSecondaryMonitorWithNegativeX_StaysInItsWorkArea()
    {
        // Windows allows monitors arranged to the LEFT of the primary, producing negative X
        // coordinates. The helper must use the supplied work-area coordinates verbatim and
        // not assume the desktop starts at 0,0.
        var leftWork = new Rect(-1920, 0, 1920, 1040);
        var leftBar = new Rect(-1920, 1040, 1920, 40);
        var anchor = Centre(leftWork);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(400, 300), leftBar, TaskbarEdge.Bottom, leftWork, PopupPositionPreference.Auto, anchor);

        placement.Left.Should().Be(anchor.X - 400 / 2);
        placement.Top.Should().Be(1040 - 300 - TaskbarPositionHelper.Margin);
    }

    [Fact]
    public void Placement_PopupExactlyFillsWorkAreaWidth_LandsAtMonitorLeft()
    {
        // Boundary: popup width equals work-area width exactly — Left must be the monitor's
        // own Left edge (which is 0 here), not negative.
        var workArea = new Rect(0, 0, 400, 1040);
        var bottomBar = new Rect(0, 1040, 400, 40);
        var anchor = Centre(workArea);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(400, 300), bottomBar, TaskbarEdge.Bottom, workArea, PopupPositionPreference.Auto, anchor);

        placement.Left.Should().Be(0);
    }

    [Theory]
    [InlineData(PopupPositionPreference.Above)]
    [InlineData(PopupPositionPreference.Below)]
    [InlineData(PopupPositionPreference.Auto)]
    public void Placement_StaysWithinWorkArea_AcrossAllPreferencesOnStandardLayout(PopupPositionPreference preference)
    {
        // Sweep: a standard 1080p single-monitor layout must produce placement entirely
        // inside the work area regardless of the user's vertical-anchor preference.
        var workArea = new Rect(0, 0, 1920, 1040);
        var bottomBar = new Rect(0, 1040, 1920, 40);
        var popup = new Size(400, 300);
        var anchor = Centre(workArea);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            popup, bottomBar, TaskbarEdge.Bottom, workArea, preference, anchor);

        placement.Left.Should().BeGreaterOrEqualTo(workArea.Left);
        placement.Top.Should().BeGreaterOrEqualTo(workArea.Top);
        (placement.Left + popup.Width).Should().BeLessOrEqualTo(workArea.Right);
        (placement.Top + popup.Height).Should().BeLessOrEqualTo(workArea.Bottom);
    }

    [Fact]
    public void Placement_At150PercentScale_ConvertsDevicePixelsToDips()
    {
        // 2560×1440 laptop panel at 150 % scaling: Win32 reports device pixels, WPF wants
        // DIPs (2560/1.5 ≈ 1706.67 DIPs wide). Pre-fix the device values were used verbatim,
        // pushing the popup ~33 % right/down and off-screen for right-half clicks.
        const double scale = 1.5;
        var workAreaPx = new Rect(0, 0, 2560, 1380);
        var bottomBarPx = new Rect(0, 1380, 2560, 60);
        var popup = new Size(400, 300);
        var anchorPx = new Point(1280, 1400);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            popup, bottomBarPx, TaskbarEdge.Bottom, workAreaPx, PopupPositionPreference.Auto, anchorPx, scale);

        // Horizontally centred on the anchor in DIP space: 1280/1.5 - 200
        placement.Left.Should().BeApproximately(1280 / scale - popup.Width / 2, 0.01);
        // Above the taskbar in DIP space: 1380/1.5 - 300 - margin
        placement.Top.Should().BeApproximately(1380 / scale - popup.Height - TaskbarPositionHelper.Margin, 0.01);
        // And on-screen in DIP space
        (placement.Left + popup.Width).Should().BeLessOrEqualTo(2560 / scale);
        (placement.Top + popup.Height).Should().BeLessOrEqualTo(1380 / scale);
    }

    [Fact]
    public void Placement_At150PercentScale_RightEdgeClick_StaysOnScreen()
    {
        // The exact off-screen repro: click near the right edge of a scaled display. In
        // device pixels the anchor X (2500) already exceeds the DIP screen width (1706.67),
        // so the unconverted math placed the popup entirely off-screen.
        const double scale = 1.5;
        var workAreaPx = new Rect(0, 0, 2560, 1380);
        var bottomBarPx = new Rect(0, 1380, 2560, 60);
        var popup = new Size(400, 300);
        var anchorPx = new Point(2500, 1400);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            popup, bottomBarPx, TaskbarEdge.Bottom, workAreaPx, PopupPositionPreference.Auto, anchorPx, scale);

        (placement.Left + popup.Width).Should().BeLessOrEqualTo(2560 / scale, "popup must stay inside the DIP work area");
        placement.Left.Should().BeGreaterOrEqualTo(0);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    public void Placement_InvalidDpiScale_FallsBackTo100Percent(double invalidScale)
    {
        // A broken GetDpiForMonitor result must not divide by zero / flip coordinates.
        var anchor = Centre(_monitorWorkAreaBottomBar);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(400, 300), _bottomTaskbar, TaskbarEdge.Bottom, _monitorWorkAreaBottomBar,
            PopupPositionPreference.Auto, anchor, invalidScale);

        placement.Top.Should().Be(1040 - 300 - TaskbarPositionHelper.Margin);
        placement.Left.Should().Be(anchor.X - 400 / 2);
    }
}
