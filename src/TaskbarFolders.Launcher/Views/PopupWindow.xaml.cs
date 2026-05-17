using System;
using System.Windows;
using TaskbarFolders.Launcher.Configuration;

namespace TaskbarFolders.Launcher.Views;

/// <summary>
/// Popup window displayed when a taskbar group is clicked.
/// </summary>
public partial class PopupWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PopupWindow"/> class.
    /// </summary>
    /// <param name="options">Launcher options including the target group identifier.</param>
    public PopupWindow(LauncherOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        InitializeComponent();
        Title = $"TaskbarFolders [{options.GroupId}]";
    }
}
