using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using TaskbarFolders.Launcher.Services;
using TaskbarFolders.Launcher.ViewModels;
using TaskbarFolders.Shared.Configuration;
using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Launcher.Views;

/// <summary>
/// Popup window displayed when a taskbar group is clicked. Reads the user's popup-position
/// preference from the injected <see cref="AppSettings"/>, places itself anchored on the
/// cursor position at click time, plays a fade+scale open animation, and dismisses on focus
/// loss.
/// </summary>
/// <remarks>
/// v0.3+: chrome is fully transparent — no acrylic backdrop, no border, no shadow. The
/// previous TryEnableAcrylic path was removed; only the per-tile hover highlight is visible.
/// <see cref="AppSettings"/> is injected directly rather than re-loaded via
/// <see cref="IAppSettingsStore"/>; App.OnStartup loads once and registers the instance as a
/// singleton.
/// </remarks>
public partial class PopupWindow : Window
{
    /// <summary>Tile width + height in DIPs. Mirrors the Image width in the data template.</summary>
    private const int TilePx = 96;

    /// <summary>Outer padding on the popup Border in DIPs.</summary>
    private const int PaddingPx = 12;

    private readonly PopupViewModel _viewModel;
    private readonly ITaskbarPositionHelper _positionHelper;
    private readonly AppSettings _settings;
    private DispatcherTimer? _safetyTimer;

    /// <summary>Initializes a new instance of the <see cref="PopupWindow"/> class.</summary>
    public PopupWindow(
        PopupViewModel viewModel,
        ITaskbarPositionHelper positionHelper,
        AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(positionHelper);
        ArgumentNullException.ThrowIfNull(settings);

        InitializeComponent();
        _viewModel = viewModel;
        _positionHelper = positionHelper;
        _settings = settings;
        DataContext = viewModel;

        _viewModel.LaunchSucceeded += OnLaunchSucceeded;
        Closed += OnClosed;
        SourceInitialized += OnSourceInitialized;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        // Compute explicit Width + Height from the bound grid metrics — skips the SizeToContent
        // measure pass that pre-v0.4 added ~5-10 ms before placement could be computed. Empty
        // / unavailable groups fall back to MinHeight, keeping the banner-only layout centred.
        var cols = Math.Max(_viewModel.Columns, 1);
        var rows = (_viewModel.Apps.Count + cols - 1) / cols;
        Width = Math.Clamp(cols * TilePx + 2 * PaddingPx, MinWidth, MaxWidth);
        Height = Math.Clamp(Math.Max(rows, 1) * TilePx + 2 * PaddingPx, MinHeight, MaxHeight);

        // Measure pass for hit-test rect correctness on the now-explicit size.
        UpdateLayout();

        var placement = _positionHelper.ComputePlacement(new Size(Width, Height), _settings.PopupPosition);
        Left = placement.Left;
        Top = placement.Top;

        // Set the ScaleTransform pivot to bottom-centre so the open animation grows the popup
        // up out of the clicked tile (which sits directly below the popup centre per
        // TaskbarPositionHelper). XAML keeps CenterX/Y=0 as placeholders; they MUST be set
        // before the storyboard fires.
        if (RenderTransform is ScaleTransform scale)
        {
            scale.CenterX = Width / 2.0;
            scale.CenterY = Height;
        }

        if (_settings.EnableAnimations && TryFindResource("OpenAnimation") is Storyboard storyboard)
        {
            ScheduleAnimationOnFirstRender(storyboard);
        }
        else
        {
            // Either animations are disabled OR the storyboard resource was not found.
            // The XAML defaults Opacity=0 + ScaleX/Y=0.5 mean a missing storyboard would
            // leave the popup permanently invisible — snap to the end state so the user
            // always sees the popup, regardless of resource lookup outcome.
            SnapToEndState();
        }
    }

    private void SnapToEndState()
    {
        if (FindName("ChromeRoot") is Border chrome)
        {
            chrome.Opacity = 1;
        }
        if (RenderTransform is ScaleTransform scale)
        {
            scale.ScaleX = 1;
            scale.ScaleY = 1;
        }
    }

    /// <summary>
    /// Schedules <see cref="Storyboard.Begin(System.Windows.FrameworkElement)"/> on the next
    /// dispatcher Render cycle and arms a 500 ms safety-net that force-snaps to the end state
    /// if the popup is still invisible. v0.4.1 used <see cref="CompositionTarget.Rendering"/>,
    /// but Win11 24H2 can skip the composition pass entirely for fully-transparent windows —
    /// <c>Rendering</c> never fires and the popup stays invisible forever. A
    /// <see cref="Dispatcher.BeginInvoke(DispatcherPriority, Delegate)"/> at Render priority
    /// always runs regardless of paint state. <c>Storyboard.SetTarget</c> on the
    /// <c>ChromeRoot</c> opacity child resolves the visual-tree element directly instead of
    /// going through the resource-scope <c>TargetName</c> lookup, which can silently no-op
    /// when the storyboard lives in <c>Window.Resources</c>. The 500 ms timer then guarantees
    /// the popup is visible no matter which corner of the WPF animation pipeline fails.
    /// </summary>
    private void ScheduleAnimationOnFirstRender(Storyboard storyboard)
    {
        if (FindName("ChromeRoot") is not Border chrome)
        {
            // No chrome to animate — collapsing to the end state still gives the user a
            // popup; missing chrome is a far worse bug than a missed animation.
            SnapToEndState();
            return;
        }

        foreach (var anim in storyboard.Children)
        {
            if (Storyboard.GetTargetName(anim) == "ChromeRoot")
            {
                Storyboard.SetTarget(anim, chrome);
            }
        }

        Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => storyboard.Begin(this)));

        // Stored in a field so OnClosed can stop the timer if the popup is dismissed before
        // 500 ms (Deactivated / LaunchSucceeded close the window). Without that, the captured
        // chrome reference keeps the window alive until the tick fires and writes opacity on
        // a detached visual tree.
        _safetyTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(500),
        };
        _safetyTimer.Tick += (_, _) =>
        {
            _safetyTimer?.Stop();
            if (chrome.Opacity < 0.5)
            {
                SnapToEndState();
            }
        };
        _safetyTimer.Start();
    }

    private void OnDeactivated(object? sender, EventArgs e) => Close();

    private void OnLaunchSucceeded(object? sender, EventArgs e) => Close();

    private void OnClosed(object? sender, EventArgs e)
    {
        // Cancel any in-flight icon-load tasks so post-close task completions cannot
        // mutate the now-detached view model.
        _viewModel.CancelIconLoad();
        // Disarm the visibility safety-net if the popup closes before it fires (e.g.,
        // Deactivated dismissal within 500 ms). Letting it tick after Close would write
        // opacity on a detached visual tree.
        _safetyTimer?.Stop();
        _safetyTimer = null;
        _viewModel.LaunchSucceeded -= OnLaunchSucceeded;
        Closed -= OnClosed;
        SourceInitialized -= OnSourceInitialized;
    }
}
