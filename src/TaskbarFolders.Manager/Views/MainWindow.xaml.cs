using System.Windows;
using TaskbarFolders.Manager.ViewModels;

namespace TaskbarFolders.Manager.Views;

/// <summary>
/// Main window of the TaskbarFolders Manager application.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadGroupsAsync().ConfigureAwait(true);
    }
}
