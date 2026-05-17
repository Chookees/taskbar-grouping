using System;
using System.Threading.Tasks;
using System.Windows;
using TaskbarFolders.Launcher.Services;
using TaskbarFolders.Launcher.ViewModels;
using TaskbarFolders.Shared.Configuration;

namespace TaskbarFolders.Launcher.Views;

/// <summary>
/// Popup window displayed when a taskbar group is clicked. Reads the user's popup-position
/// preference from <see cref="IAppSettingsStore"/>, places itself adjacent to the taskbar on
/// the monitor under the cursor, and subscribes to
/// <see cref="PopupViewModel.LaunchSucceeded"/> so a successful app launch dismisses the popup.
/// </summary>
public partial class PopupWindow : Window
{
    private readonly PopupViewModel _viewModel;
    private readonly ITaskbarPositionHelper _positionHelper;
    private readonly IAppSettingsStore _settingsStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="PopupWindow"/> class.
    /// </summary>
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
    }

    private async void OnSourceInitialized(object? sender, EventArgs e)
    {
        await PositionToTaskbarAsync().ConfigureAwait(true);
    }

    private async Task PositionToTaskbarAsync()
    {
        var settings = await _settingsStore.LoadAsync().ConfigureAwait(true);

        var size = new Size(Width, Height);
        var placement = _positionHelper.ComputePlacement(size, settings.PopupPosition);
        Left = placement.Left;
        Top = placement.Top;
    }

    private void OnLaunchSucceeded(object? sender, EventArgs e) => Close();

    private void OnClosed(object? sender, EventArgs e)
    {
        _viewModel.LaunchSucceeded -= OnLaunchSucceeded;
        Closed -= OnClosed;
        SourceInitialized -= OnSourceInitialized;
    }
}
