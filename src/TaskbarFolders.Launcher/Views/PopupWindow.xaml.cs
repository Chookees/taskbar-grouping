using System;
using System.Windows;
using System.Windows.Media.Animation;
using TaskbarFolders.Launcher.Services;
using TaskbarFolders.Launcher.ViewModels;
using TaskbarFolders.Shared.Configuration;
using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Launcher.Views;

/// <summary>
/// Popup window displayed when a taskbar group is clicked. Reads the user's popup-position
/// preference from <see cref="IAppSettingsStore"/>, places itself anchored on the cursor
/// position at click time, plays a fade+scale open animation, and dismisses on focus loss.
/// </summary>
/// <remarks>
/// v0.3+: chrome is fully transparent — no acrylic backdrop, no border, no shadow. The
/// previous TryEnableAcrylic path was removed; only the per-tile hover highlight is visible.
/// AppSettings is now injected directly (v0.3+) rather than re-loaded via IAppSettingsStore;
/// App.OnStartup loads once and registers the instance as a singleton.
/// </remarks>
public partial class PopupWindow : Window
{
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
        Loaded += OnLoaded;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        // Ensure layout has measured so SizeToContent has set Width/Height before placing.
        UpdateLayout();

        var size = new Size(ActualWidth > 0 ? ActualWidth : Width, ActualHeight > 0 ? ActualHeight : Height);
        var placement = _positionHelper.ComputePlacement(size, _settings.PopupPosition);
        Left = placement.Left;
        Top = placement.Top;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (!_settings.EnableAnimations)
        {
            return;
        }

        if (TryFindResource("OpenAnimation") is Storyboard storyboard)
        {
            storyboard.Begin(this);
        }
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
        Loaded -= OnLoaded;
    }
}
