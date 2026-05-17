using System.Windows;
using FluentAssertions;
using TaskbarFolders.Launcher.Services;
using TaskbarFolders.Shared.Models;
using Xunit;

namespace TaskbarFolders.Launcher.Tests.Services;

public class TaskbarPositionHelperTests
{
    // 1920×1080 primary monitor; bottom taskbar 40px tall.
    private static readonly Rect _monitorFull = new(0, 0, 1920, 1080);
    private static readonly Rect _monitorWorkAreaBottomBar = new(0, 0, 1920, 1040);
    private static readonly Rect _bottomTaskbar = new(0, 1040, 1920, 40);

    [Fact]
    public void BottomTaskbar_Auto_PlacesPopupAbove_AndHorizontallyCentred()
    {
        var size = new Size(400, 300);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            size, _bottomTaskbar, TaskbarEdge.Bottom, _monitorWorkAreaBottomBar, PopupPositionPreference.Auto);

        // Above the taskbar, with the margin
        placement.Top.Should().Be(1040 - 300 - TaskbarPositionHelper.Margin);
        // Centred horizontally over the taskbar
        placement.Left.Should().Be((1920 - 400) / 2);
    }

    [Fact]
    public void TopTaskbar_Auto_PlacesPopupBelow()
    {
        var topTaskbar = new Rect(0, 0, 1920, 40);
        var workArea = new Rect(0, 40, 1920, 1040);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(400, 300), topTaskbar, TaskbarEdge.Top, workArea, PopupPositionPreference.Auto);

        placement.Top.Should().Be(40 + TaskbarPositionHelper.Margin);
    }

    [Fact]
    public void BottomTaskbar_BelowPreference_OverridesAutoAndPlacesPopupBelowTaskbar()
    {
        // Bottom-taskbar Auto puts the popup above. Preference=Below forces it below
        // (which is unusual but documented). Should be clamped to the work area.
        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(400, 300), _bottomTaskbar, TaskbarEdge.Bottom, _monitorWorkAreaBottomBar, PopupPositionPreference.Below);

        // Top should be at taskbar.Bottom + margin = 1080 + 8 = 1088 → clamped to fit
        placement.Top.Should().BeLessOrEqualTo(_monitorWorkAreaBottomBar.Bottom - 300);
    }

    [Fact]
    public void LeftTaskbar_PlacesPopupToTheRight_AndVerticallyCentred()
    {
        var leftTaskbar = new Rect(0, 0, 60, 1080);
        var workArea = new Rect(60, 0, 1860, 1080);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(400, 300), leftTaskbar, TaskbarEdge.Left, workArea, PopupPositionPreference.Auto);

        placement.Left.Should().Be(60 + TaskbarPositionHelper.Margin);
        placement.Top.Should().Be((1080 - 300) / 2);
    }

    [Fact]
    public void RightTaskbar_PlacesPopupToTheLeft_AndVerticallyCentred()
    {
        var rightTaskbar = new Rect(1860, 0, 60, 1080);
        var workArea = new Rect(0, 0, 1860, 1080);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(400, 300), rightTaskbar, TaskbarEdge.Right, workArea, PopupPositionPreference.Auto);

        placement.Left.Should().Be(1860 - 400 - TaskbarPositionHelper.Margin);
        placement.Top.Should().Be((1080 - 300) / 2);
    }

    [Fact]
    public void Placement_IsClampedToWorkArea_WhenPopupExceedsMonitorWidth()
    {
        // Tiny monitor (800x600) with a wide popup (1000) — left should clamp to 0.
        var smallWorkArea = new Rect(0, 0, 800, 560);
        var bottomBar = new Rect(0, 560, 800, 40);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(1000, 300), bottomBar, TaskbarEdge.Bottom, smallWorkArea, PopupPositionPreference.Auto);

        placement.Left.Should().Be(0, "popup wider than monitor → clamp to work-area left");
    }

    [Fact]
    public void Placement_OnSecondaryMonitor_UsesItsWorkArea_NotPrimary()
    {
        // Secondary monitor at x=1920, 1920x1080, with bottom taskbar (rare but supported)
        var secondaryWork = new Rect(1920, 0, 1920, 1040);
        var secondaryBar = new Rect(1920, 1040, 1920, 40);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(400, 300), secondaryBar, TaskbarEdge.Bottom, secondaryWork, PopupPositionPreference.Auto);

        // Centred on the secondary monitor's X span
        placement.Left.Should().Be(1920 + (1920 - 400) / 2);
        placement.Top.Should().Be(1040 - 300 - TaskbarPositionHelper.Margin);
    }

    [Fact]
    public void Placement_ClampsTop_WhenPopupTallerThanWorkArea()
    {
        // Pathological: popup taller than the work area. Top should clamp to work-area top
        // so the header is still visible (worse than scrolling but better than off-screen).
        var workArea = new Rect(0, 0, 1920, 400);
        var bottomBar = new Rect(0, 400, 1920, 40);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(400, 700), bottomBar, TaskbarEdge.Bottom, workArea, PopupPositionPreference.Auto);

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

        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(400, 300), leftBar, TaskbarEdge.Bottom, leftWork, PopupPositionPreference.Auto);

        // Centred on the negative-X monitor span
        placement.Left.Should().Be(-1920 + (1920 - 400) / 2);
        placement.Top.Should().Be(1040 - 300 - TaskbarPositionHelper.Margin);
    }

    [Fact]
    public void Placement_PopupExactlyFillsWorkAreaWidth_LandsAtMonitorLeft()
    {
        // Boundary: popup width equals work-area width exactly — Left must be the monitor's
        // own Left edge (which is 0 here), not negative.
        var workArea = new Rect(0, 0, 400, 1040);
        var bottomBar = new Rect(0, 1040, 400, 40);

        var placement = TaskbarPositionHelper.CalculatePlacement(
            new Size(400, 300), bottomBar, TaskbarEdge.Bottom, workArea, PopupPositionPreference.Auto);

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

        var placement = TaskbarPositionHelper.CalculatePlacement(
            popup, bottomBar, TaskbarEdge.Bottom, workArea, preference);

        placement.Left.Should().BeGreaterOrEqualTo(workArea.Left);
        placement.Top.Should().BeGreaterOrEqualTo(workArea.Top);
        (placement.Left + popup.Width).Should().BeLessOrEqualTo(workArea.Right);
        (placement.Top + popup.Height).Should().BeLessOrEqualTo(workArea.Bottom);
    }
}
