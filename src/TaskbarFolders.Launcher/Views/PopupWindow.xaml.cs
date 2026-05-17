using System;
using System.Windows;
using TaskbarFolders.Launcher.ViewModels;

namespace TaskbarFolders.Launcher.Views;

/// <summary>
/// Popup window displayed when a taskbar group is clicked. Subscribes to
/// <see cref="PopupViewModel.LaunchSucceeded"/> so a successful app launch dismisses the popup.
/// </summary>
public partial class PopupWindow : Window
{
    private readonly PopupViewModel _viewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="PopupWindow"/> class.
    /// </summary>
    /// <param name="viewModel">View model resolved from the DI container.</param>
    public PopupWindow(PopupViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        _viewModel.LaunchSucceeded += OnLaunchSucceeded;
        Closed += OnClosed;
    }

    private void OnLaunchSucceeded(object? sender, EventArgs e) => Close();

    private void OnClosed(object? sender, EventArgs e)
    {
        _viewModel.LaunchSucceeded -= OnLaunchSucceeded;
        Closed -= OnClosed;
    }
}
