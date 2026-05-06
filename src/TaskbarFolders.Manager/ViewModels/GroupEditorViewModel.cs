using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TaskbarFolders.Core.Icons;
using TaskbarFolders.Manager.Services;
using TaskbarFolders.Shared.Configuration;
using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Manager.ViewModels;

/// <summary>
/// ViewModel for editing a single group's configuration.
/// </summary>
public sealed class GroupEditorViewModel : ViewModelBase
{
    private readonly IGroupConfigStore _configStore;
    private readonly IIconExtractor _iconExtractor;
    private readonly ICompositeIconGenerator _compositeGenerator;
    private readonly LauncherGenerator _launcherGenerator;
    private readonly GroupConfig _config;
    private string _groupName;
    private int _columns;
    private BitmapSource? _compositeIcon;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupEditorViewModel"/> class.
    /// </summary>
    public GroupEditorViewModel(
        GroupConfig config,
        IGroupConfigStore configStore,
        IIconExtractor iconExtractor,
        ICompositeIconGenerator compositeGenerator,
        LauncherGenerator launcherGenerator)
    {
        _config = config;
        _configStore = configStore;
        _iconExtractor = iconExtractor;
        _compositeGenerator = compositeGenerator;
        _launcherGenerator = launcherGenerator;
        _groupName = config.GroupName;
        _columns = config.Columns;

        SaveCommand = new RelayCommand(async _ => await SaveAsync().ConfigureAwait(true));
        AddAppCommand = new RelayCommand(OnAddApp);
        RemoveAppCommand = new RelayCommand(OnRemoveApp);
        DropFilesCommand = new RelayCommand(OnDropFiles);

        LoadApps();
        UpdateCompositeIcon();
    }

    /// <summary>
    /// The group's unique ID.
    /// </summary>
    public string GroupId => _config.Id;

    /// <summary>
    /// The display name of the group.
    /// </summary>
    public string GroupName
    {
        get => _groupName;
        set
        {
            if (SetProperty(ref _groupName, value))
                _config.GroupName = value;
        }
    }

    /// <summary>
    /// Number of columns in the popup grid.
    /// </summary>
    public int Columns
    {
        get => _columns;
        set
        {
            if (SetProperty(ref _columns, value))
                _config.Columns = value;
        }
    }

    /// <summary>
    /// The generated composite icon preview.
    /// </summary>
    public BitmapSource? CompositeIcon
    {
        get => _compositeIcon;
        private set => SetProperty(ref _compositeIcon, value);
    }

    /// <summary>
    /// Applications in this group.
    /// </summary>
    public ObservableCollection<AppEntryViewModel> Apps { get; } = [];

    /// <summary>
    /// Command to save the group.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Command to add an app via file path.
    /// </summary>
    public ICommand AddAppCommand { get; }

    /// <summary>
    /// Command to remove an app from the group.
    /// </summary>
    public ICommand RemoveAppCommand { get; }

    /// <summary>
    /// Command invoked when files are dropped onto the editor.
    /// </summary>
    public ICommand DropFilesCommand { get; }

    /// <summary>
    /// Available column count options.
    /// </summary>
    public static int[] ColumnOptions => [2, 3, 4, 5];

    /// <summary>
    /// Adds files (exe/lnk) dropped onto the group editor.
    /// </summary>
    public void AddFiles(string[] filePaths)
    {
        foreach (string path in filePaths)
        {
            string ext = Path.GetExtension(path).ToUpperInvariant();
            if (ext is not ".EXE" and not ".LNK")
                continue;

            string name = Path.GetFileNameWithoutExtension(path);
            var entry = new AppEntry { Name = name, Path = path };
            AddAppEntry(entry);
        }

        UpdateCompositeIcon();
    }

    private void OnDropFiles(object? parameter)
    {
        if (parameter is string[] files)
            AddFiles(files);
    }

    private void OnAddApp(object? parameter)
    {
        if (parameter is string path && File.Exists(path))
        {
            string name = Path.GetFileNameWithoutExtension(path);
            var entry = new AppEntry { Name = name, Path = path };
            AddAppEntry(entry);
            UpdateCompositeIcon();
        }
    }

    private void OnRemoveApp(object? parameter)
    {
        if (parameter is not AppEntryViewModel vm)
            return;

        Apps.Remove(vm);
        _config.Apps.Remove(vm.Model);
        UpdateCompositeIcon();
    }

    private void AddAppEntry(AppEntry entry)
    {
        var vm = new AppEntryViewModel(entry);

        try
        {
            vm.Icon = _iconExtractor.ExtractIcon(entry.Path);
        }
        catch (Exception)
        {
            // fallback: null icon
        }

        Apps.Add(vm);
        _config.Apps.Add(entry);
    }

    private void LoadApps()
    {
        foreach (AppEntry app in _config.Apps)
        {
            var vm = new AppEntryViewModel(app);

            try
            {
                vm.Icon = _iconExtractor.ExtractIcon(app.IconPath ?? app.Path);
            }
            catch (Exception)
            {
                // fallback
            }

            Apps.Add(vm);
        }
    }

    private void UpdateCompositeIcon()
    {
        var icons = Apps
            .Where(a => a.Icon is not null)
            .Take(4)
            .Select(a => a.Icon!)
            .ToList();

        if (icons.Count == 0)
        {
            CompositeIcon = null;
            return;
        }

        CompositeIcon = _compositeGenerator.GenerateComposite(icons);
    }

    private async Task SaveAsync()
    {
        await _configStore.SaveAsync(_config).ConfigureAwait(true);

        if (CompositeIcon is not null)
        {
            _launcherGenerator.GenerateGroupIcon(_config.Id, CompositeIcon);
        }
    }
}
