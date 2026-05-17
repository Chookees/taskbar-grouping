using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using TaskbarFolders.Core.Icons;
using TaskbarFolders.Launcher.Configuration;
using TaskbarFolders.Launcher.Services;
using TaskbarFolders.Shared.Configuration;
using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Launcher.ViewModels;

/// <summary>
/// Backing view model for <see cref="Views.PopupWindow"/>. Loads the group identified by
/// <see cref="LauncherOptions.GroupId"/>, populates an <see cref="ObservableCollection{T}"/>
/// of <see cref="PopupAppViewModel"/>s with extracted icons, and handles clicks.
/// </summary>
public sealed partial class PopupViewModel : ObservableObject
{
    /// <summary>Pixel size requested from the icon extractor for tiles.</summary>
    public const int IconSize = 64;

    private readonly IGroupConfigStore _store;
    private readonly IIconExtractor _extractor;
    private readonly IIconCache _cache;
    private readonly IProcessLauncher _launcher;
    private readonly LauncherOptions _options;
    private readonly ILogger<PopupViewModel>? _logger;

    /// <summary>Initializes a new instance.</summary>
    public PopupViewModel(
        IGroupConfigStore store,
        IIconExtractor extractor,
        IIconCache cache,
        IProcessLauncher launcher,
        LauncherOptions options,
        ILogger<PopupViewModel>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(extractor);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(launcher);
        ArgumentNullException.ThrowIfNull(options);

        _store = store;
        _extractor = extractor;
        _cache = cache;
        _launcher = launcher;
        _options = options;
        _logger = logger;
    }

    /// <summary>Display name of the loaded group; empty string until <see cref="LoadAsync"/> completes.</summary>
    [ObservableProperty]
    private string _groupName = string.Empty;

    /// <summary>Number of grid columns for the popup; mirrors <see cref="GroupConfig.Columns"/>.</summary>
    [ObservableProperty]
    private int _columns = 3;

    /// <summary>True when the loaded group does not exist or is empty — used by the UI to show an inline error.</summary>
    [ObservableProperty]
    private bool _isUnavailable;

    /// <summary>App entries to render. Empty until <see cref="LoadAsync"/> populates it.</summary>
    public ObservableCollection<PopupAppViewModel> Apps { get; } = [];

    /// <summary>Raised after a successful launch so the window can dismiss itself.</summary>
    public event EventHandler? LaunchSucceeded;

    /// <summary>
    /// Reads the group config and extracts each app's icon. Must be called once after
    /// construction (typically from the window's Loaded handler or App.OnStartup).
    /// </summary>
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var config = await _store.LoadAsync(_options.GroupId, cancellationToken).ConfigureAwait(true);
        if (config is null)
        {
            _logger?.LogError("Launcher started for unknown group {GroupId}.", _options.GroupId);
            IsUnavailable = true;
            GroupName = "(Unknown group)";
            return;
        }

        GroupName = config.GroupName;
        Columns = config.Columns;

        Apps.Clear();
        foreach (var entry in config.Apps)
        {
            var app = new PopupAppViewModel(entry)
            {
                Icon = LoadIcon(entry.Path),
            };
            Apps.Add(app);
        }

        if (Apps.Count == 0)
        {
            IsUnavailable = true;
            _logger?.LogWarning("Group {GroupId} is empty.", _options.GroupId);
        }
    }

    [RelayCommand]
    private void LaunchApp(PopupAppViewModel? app)
    {
        if (app is null)
        {
            return;
        }

        var ok = _launcher.Launch(app.Path, app.Arguments);
        if (ok)
        {
            _logger?.LogInformation("Launched {Path} from group {GroupId}.", app.Path, _options.GroupId);
            LaunchSucceeded?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            _logger?.LogWarning("Failed to launch {Path}; popup stays open.", app.Path);
        }
    }

    private System.Windows.Media.Imaging.BitmapSource? LoadIcon(string path)
    {
        if (_cache.TryGet(path, IconSize, out var cached))
        {
            return cached;
        }

        var icon = _extractor.ExtractIcon(path, IconSize);
        if (icon is not null)
        {
            _cache.Set(path, IconSize, icon);
        }
        return icon;
    }
}
