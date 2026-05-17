using System;
using System.Windows;
using System.Windows.Input;
using TaskbarFolders.Manager.ViewModels;

namespace TaskbarFolders.Manager.Views;

/// <summary>
/// Main window of the TaskbarFolders Manager application.
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

    /// <summary>
    /// Treats Enter in the "new group name" TextBox as a click on Add.
    /// Keeps no business logic in the code-behind — it just routes the input event
    /// to the existing view-model command.
    /// </summary>
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
}
