using System;
using System.Windows;
using TaskbarFolders.Manager.ViewModels;

namespace TaskbarFolders.Manager.Views;

/// <summary>
/// Modal settings window. The DI-resolved <see cref="SettingsViewModel"/> is loaded once
/// on construction; the dialog remains open until the user clicks Close.
/// </summary>
public partial class SettingsWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsWindow"/> class.
    /// </summary>
    /// <param name="viewModel">View model resolved from the DI container.</param>
    public SettingsWindow(SettingsViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();
        DataContext = viewModel;
    }
}
