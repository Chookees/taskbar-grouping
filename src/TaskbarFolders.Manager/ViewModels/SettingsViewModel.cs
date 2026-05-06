using System.Windows.Input;
using TaskbarFolders.Shared.Configuration;
using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Manager.ViewModels;

/// <summary>
/// ViewModel for the application settings view.
/// </summary>
public sealed class SettingsViewModel : ViewModelBase
{
    private readonly IAppSettingsStore _settingsStore;
    private bool _autoStart;
    private string _theme = "system";
    private bool _enableAnimations = true;
    private string _popupPosition = "auto";

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
    /// </summary>
    public SettingsViewModel(IAppSettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
        SaveCommand = new RelayCommand(async _ => await SaveAsync().ConfigureAwait(true));
    }

    /// <summary>
    /// Whether to start with Windows.
    /// </summary>
    public bool AutoStart
    {
        get => _autoStart;
        set => SetProperty(ref _autoStart, value);
    }

    /// <summary>
    /// Theme selection (system, light, dark).
    /// </summary>
    public string Theme
    {
        get => _theme;
        set => SetProperty(ref _theme, value);
    }

    /// <summary>
    /// Whether popup animations are enabled.
    /// </summary>
    public bool EnableAnimations
    {
        get => _enableAnimations;
        set => SetProperty(ref _enableAnimations, value);
    }

    /// <summary>
    /// Popup position preference.
    /// </summary>
    public string PopupPosition
    {
        get => _popupPosition;
        set => SetProperty(ref _popupPosition, value);
    }

    /// <summary>
    /// Command to save settings.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Available theme options.
    /// </summary>
    public static string[] ThemeOptions => ["system", "light", "dark"];

    /// <summary>
    /// Available popup position options.
    /// </summary>
    public static string[] PopupPositionOptions => ["auto", "above", "below"];

    /// <summary>
    /// Loads settings from persistent storage.
    /// </summary>
    public async Task LoadAsync()
    {
        AppSettings settings = await _settingsStore.LoadAsync().ConfigureAwait(true);
        AutoStart = settings.AutoStart;
        Theme = settings.Theme;
        EnableAnimations = settings.EnableAnimations;
        PopupPosition = settings.PopupPosition;
    }

    private async Task SaveAsync()
    {
        var settings = new AppSettings
        {
            AutoStart = AutoStart,
            Theme = Theme,
            EnableAnimations = EnableAnimations,
            PopupPosition = PopupPosition,
        };

        await _settingsStore.SaveAsync(settings).ConfigureAwait(true);
    }
}
