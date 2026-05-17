using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using TaskbarFolders.Launcher.Interop;
using TaskbarFolders.Launcher.Services;
using TaskbarFolders.Launcher.ViewModels;
using TaskbarFolders.Shared.Configuration;
using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Launcher.Views;

/// <summary>
/// Popup window displayed when a taskbar group is clicked. Reads the user's popup-position
/// preference from <see cref="IAppSettingsStore"/>, places itself adjacent to the taskbar on
/// the monitor under the cursor, attempts to enable Acrylic on Win11 22H2+, plays a fade+scale
/// open animation, and dismisses on focus loss.
/// </summary>
public partial class PopupWindow : Window
{
    private readonly PopupViewModel _viewModel;
    private readonly ITaskbarPositionHelper _positionHelper;
    private readonly IAppSettingsStore _settingsStore;

    private bool _animationsEnabled = true;

    /// <summary>Initializes a new instance of the <see cref="PopupWindow"/> class.</summary>
    public PopupWindow(
        PopupViewModel viewModel,
        ITaskbarPositionHelper positionHelper,
        IAppSettingsStore settingsStore)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(positionHelper);
        ArgumentNullException.ThrowIfNull(settingsStore);

        InitializeComponent();
        _viewModel = viewModel;
        _positionHelper = positionHelper;
        _settingsStore = settingsStore;
        DataContext = viewModel;

        _viewModel.LaunchSucceeded += OnLaunchSucceeded;
        Closed += OnClosed;
        SourceInitialized += OnSourceInitialized;
        Loaded += OnLoaded;
    }

    private async void OnSourceInitialized(object? sender, EventArgs e)
    {
        TryEnableAcrylic();
        await PositionAndConfigureAsync().ConfigureAwait(true);
    }

    private void TryEnableAcrylic()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        var backdrop = NativeMethods.DWMSBT_TRANSIENTWINDOW; // Acrylic
        // Returns non-zero HRESULT on pre-22H2 Windows — silently fall back to the
        // semi-transparent themed brush configured in XAML.
        _ = NativeMethods.DwmSetWindowAttribute(hwnd, NativeMethods.DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, sizeof(int));
    }

    private async Task PositionAndConfigureAsync()
    {
        var settings = await _settingsStore.LoadAsync().ConfigureAwait(true);
        _animationsEnabled = settings.EnableAnimations;

        // Ensure layout has measured so SizeToContent has set Width/Height before placing.
        UpdateLayout();

        var size = new Size(ActualWidth > 0 ? ActualWidth : Width, ActualHeight > 0 ? ActualHeight : Height);
        var placement = _positionHelper.ComputePlacement(size, settings.PopupPosition);
        Left = placement.Left;
        Top = placement.Top;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (!_animationsEnabled)
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
        _viewModel.LaunchSucceeded -= OnLaunchSucceeded;
        Closed -= OnClosed;
        SourceInitialized -= OnSourceInitialized;
        Loaded -= OnLoaded;
    }
}
