using CommunityToolkit.Mvvm.ComponentModel;

namespace TaskbarFolders.Manager.ViewModels;

/// <summary>
/// View model for <see cref="Views.MainWindow"/>.
/// Carries the application title and, in later milestones, the list of groups.
/// </summary>
public sealed partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "TaskbarFolders Manager";
}
