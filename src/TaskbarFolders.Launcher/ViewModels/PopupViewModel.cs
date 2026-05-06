using System.Collections.ObjectModel;
using System.Windows.Input;
using TaskbarFolders.Core.Icons;
using TaskbarFolders.Launcher.Services;
using TaskbarFolders.Shared.Configuration;
using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Launcher.ViewModels;

/// <summary>
/// ViewModel for the popup window showing grouped applications.
/// </summary>
public sealed class PopupViewModel : ViewModelBase
{
    private readonly IGroupConfigStore _configStore;
    private readonly IIconExtractor _iconExtractor;
    private readonly ProcessLauncher _processLauncher;
    private string _groupName = string.Empty;
    private int _columns = 3;

    /// <summary>
    /// Initializes a new instance of the <see cref="PopupViewModel"/> class.
    /// </summary>
    public PopupViewModel(
        IGroupConfigStore configStore,
        IIconExtractor iconExtractor,
        ProcessLauncher processLauncher)
    {
        _configStore = configStore;
        _iconExtractor = iconExtractor;
        _processLauncher = processLauncher;

        LaunchAppCommand = new RelayCommand(OnLaunchApp);
    }

    /// <summary>
    /// The display name of the group.
    /// </summary>
    public string GroupName
    {
        get => _groupName;
        private set => SetProperty(ref _groupName, value);
    }

    /// <summary>
    /// Number of columns for the grid layout.
    /// </summary>
    public int Columns
    {
        get => _columns;
        private set => SetProperty(ref _columns, value);
    }

    /// <summary>
    /// The application entries to display.
    /// </summary>
    public ObservableCollection<AppEntryViewModel> Apps { get; } = [];

    /// <summary>
    /// Command to launch a selected application.
    /// </summary>
    public ICommand LaunchAppCommand { get; }

    /// <summary>
    /// Loads a group configuration and populates the apps list.
    /// </summary>
    /// <param name="groupId">The group ID to load.</param>
    public async Task LoadGroupAsync(string groupId)
    {
        GroupConfig? config = await _configStore.LoadAsync(groupId).ConfigureAwait(true);
        if (config is null)
            return;

        GroupName = config.GroupName;
        Columns = config.Columns;
        Apps.Clear();

        foreach (AppEntry app in config.Apps)
        {
            var vm = new AppEntryViewModel(app);

            try
            {
                vm.Icon = _iconExtractor.ExtractIcon(app.IconPath ?? app.Path);
            }
            catch (Exception)
            {
                // fallback: icon stays null
            }

            Apps.Add(vm);
        }
    }

    private void OnLaunchApp(object? parameter)
    {
        if (parameter is not AppEntryViewModel app)
            return;

        _processLauncher.Launch(app.Model.Path, app.Model.Arguments);
    }
}
