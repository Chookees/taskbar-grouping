using System.Windows;
using TaskbarFolders.Launcher.Services;
using TaskbarFolders.Launcher.ViewModels;

namespace TaskbarFolders.Launcher.Views;

/// <summary>
/// Popup window displayed when a taskbar group is clicked.
/// </summary>
public partial class PopupWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PopupWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The popup ViewModel.</param>
    public PopupWindow(PopupViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Point position = TaskbarPositionHelper.GetPopupPosition(ActualWidth, ActualHeight);
        Left = position.X;
        Top = position.Y;

        Activate();
    }
}
