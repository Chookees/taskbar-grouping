using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TaskbarFolders.Manager.ViewModels;

namespace TaskbarFolders.Manager.Views;

/// <summary>
/// View for editing a group's configuration.
/// </summary>
public partial class GroupEditorView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GroupEditorView"/> class.
    /// </summary>
    public GroupEditorView()
    {
        InitializeComponent();
    }

    private void OnOpenShortcutFolder(object sender, RoutedEventArgs e)
    {
        if (DataContext is not GroupEditorViewModel vm || vm.ShortcutPath is null)
            return;

        string? directory = Path.GetDirectoryName(vm.ShortcutPath);
        if (directory is not null && Directory.Exists(directory))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{vm.ShortcutPath}\"",
                UseShellExecute = true,
            });
        }
    }
}
