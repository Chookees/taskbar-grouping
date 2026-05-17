using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
    /// Defers <see cref="Storyboard.Begin(System.Windows.FrameworkElement)"/> until the first
    /// composition frame. v0.4 fired Begin directly from <see cref="OnSourceInitialized"/>;
    /// on cold launches the WPF first paint happened AFTER the 200 ms animation timeline had
    /// already elapsed, so users only ever saw the end state. Subscribing to
    /// <see cref="CompositionTarget.Rendering"/> and starting on the next render tick
    /// guarantees the timeline begins on a frame the user will actually see.
    /// </summary>
    private void ScheduleAnimationOnFirstRender(Storyboard storyboard)
    {
        EventHandler? onFrame = null;
        onFrame = (_, _) =>
        {
            CompositionTarget.Rendering -= onFrame;
            storyboard.Begin(this);
        };
        CompositionTarget.Rendering += onFrame;
    }

    private void OnDeactivated(object? sender, EventArgs e) => Close();

    private void OnLaunchSucceeded(object? sender, EventArgs e) => Close();

    private void OnClosed(object? sender, EventArgs e)
    {
        // Cancel any in-flight icon-load tasks so post-close task completions cannot
        // mutate the now-detached view model.
        _viewModel.CancelIconLoad();
        _viewModel.LaunchSucceeded -= OnLaunchSucceeded;
        Closed -= OnClosed;
        SourceInitialized -= OnSourceInitialized;
    }
}
