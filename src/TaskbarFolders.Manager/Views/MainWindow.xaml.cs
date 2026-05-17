using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using TaskbarFolders.Manager.ViewModels;

namespace TaskbarFolders.Manager.Views;

/// <summary>
/// Main window of the TaskbarFolders Manager application. Code-behind contains only
/// input-event routing (Enter → command, drag-and-drop → command, file picker → command);
/// all business logic lives in the view models.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class
    /// and assigns the supplied view model as its data context.
    /// </summary>
    /// <param name="viewModel">View model resolved from the DI container.</param>
    public MainWindow(MainWindowViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();
        DataContext = viewModel;
    }

    private void NewGroupName_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        if (DataContext is MainWindowViewModel vm && vm.AddGroupCommand.CanExecute(null))
        {
            vm.AddGroupCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void Editor_PreviewDragOver(object sender, DragEventArgs e)
    {
        var hasFiles = e.Data.GetDataPresent(DataFormats.FileDrop);
        e.Effects = hasFiles ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void Editor_Drop(object sender, DragEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        if (e.Data.GetData(DataFormats.FileDrop) is string[] paths && paths.Length > 0)
        {
            vm.Editor.AddAppsCommand.Execute(paths);
            e.Handled = true;
        }
    }

    private void AddAppPicker_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Filter = "Applications and shortcuts|*.exe;*.lnk|Executables (*.exe)|*.exe|Shortcuts (*.lnk)|*.lnk",
            Multiselect = true,
            Title = "Add apps to group",
        };

        if (dialog.ShowDialog(this) == true && dialog.FileNames.Length > 0)
        {
            vm.Editor.AddAppsCommand.Execute(dialog.FileNames);
        }
    }
}
