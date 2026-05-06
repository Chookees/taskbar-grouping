using System.Collections.ObjectModel;
using System.Windows.Input;
using TaskbarFolders.Core.Icons;
using TaskbarFolders.Manager.Services;
using TaskbarFolders.Shared.Configuration;
using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Manager.ViewModels;

/// <summary>
/// ViewModel for the main window showing all groups.
/// </summary>
public sealed class MainViewModel : ViewModelBase
{
    private readonly IGroupConfigStore _configStore;
    private readonly IAppSettingsStore _settingsStore;
    private readonly IIconExtractor _iconExtractor;
    private readonly ICompositeIconGenerator _compositeGenerator;
    private readonly LauncherGenerator _launcherGenerator;
    private GroupEditorViewModel? _selectedGroup;
    private ViewModelBase? _currentView;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    public MainViewModel(
        IGroupConfigStore configStore,
        IAppSettingsStore settingsStore,
        IIconExtractor iconExtractor,
        ICompositeIconGenerator compositeGenerator,
        LauncherGenerator launcherGenerator)
    {
        _configStore = configStore;
        _settingsStore = settingsStore;
        _iconExtractor = iconExtractor;
        _compositeGenerator = compositeGenerator;
        _launcherGenerator = launcherGenerator;

        NewGroupCommand = new RelayCommand(_ => CreateNewGroup());
        DeleteGroupCommand = new RelayCommand(OnDeleteGroup, _ => SelectedGroup is not null);
        EditGroupCommand = new RelayCommand(OnEditGroup, _ => SelectedGroup is not null);
        OpenSettingsCommand = new RelayCommand(_ => OpenSettings());
        BackToListCommand = new RelayCommand(_ => { CurrentView = null; });
    }

    /// <summary>
    /// All groups.
    /// </summary>
    public ObservableCollection<GroupEditorViewModel> Groups { get; } = [];

    /// <summary>
    /// The currently selected group.
    /// </summary>
    public GroupEditorViewModel? SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            if (SetProperty(ref _selectedGroup, value) && value is not null)
                CurrentView = value;
        }
    }

    /// <summary>
    /// The current detail view (editor or settings). Null means list view.
    /// </summary>
    public ViewModelBase? CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    /// <summary>
    /// Command to create a new group.
    /// </summary>
    public ICommand NewGroupCommand { get; }

    /// <summary>
    /// Command to delete the selected group.
    /// </summary>
    public ICommand DeleteGroupCommand { get; }

    /// <summary>
    /// Command to edit the selected group.
    /// </summary>
    public ICommand EditGroupCommand { get; }

    /// <summary>
    /// Command to open settings.
    /// </summary>
    public ICommand OpenSettingsCommand { get; }

    /// <summary>
    /// Command to return to the group list.
    /// </summary>
    public ICommand BackToListCommand { get; }

    /// <summary>
    /// Loads all groups from persistent storage.
    /// </summary>
    public async Task LoadGroupsAsync()
    {
        IReadOnlyList<GroupConfig> configs = await _configStore.LoadAllAsync().ConfigureAwait(true);
        Groups.Clear();

        foreach (GroupConfig config in configs)
        {
            var editor = CreateEditorViewModel(config);
            Groups.Add(editor);
        }
    }

    private void CreateNewGroup()
    {
        var config = new GroupConfig { GroupName = "New Group" };
        var editor = CreateEditorViewModel(config);
        Groups.Add(editor);
        SelectedGroup = editor;
        CurrentView = editor;
    }

    private async void OnDeleteGroup(object? parameter)
    {
        if (SelectedGroup is null)
            return;

        await _configStore.DeleteAsync(SelectedGroup.GroupId).ConfigureAwait(true);
        Groups.Remove(SelectedGroup);
        SelectedGroup = null;
        CurrentView = null;
    }

    private void OnEditGroup(object? parameter)
    {
        if (SelectedGroup is not null)
            CurrentView = SelectedGroup;
    }

    private void OpenSettings()
    {
        var vm = new SettingsViewModel(_settingsStore);
        _ = vm.LoadAsync();
        CurrentView = vm;
    }

    private GroupEditorViewModel CreateEditorViewModel(GroupConfig config)
    {
        return new GroupEditorViewModel(
            config,
            _configStore,
            _iconExtractor,
            _compositeGenerator,
            _launcherGenerator);
    }
}
