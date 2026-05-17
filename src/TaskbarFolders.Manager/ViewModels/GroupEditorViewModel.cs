using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using TaskbarFolders.Core.Icons;
using TaskbarFolders.Manager.Services;
using TaskbarFolders.Shared.Configuration;
using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Manager.ViewModels;

/// <summary>
/// Detail-pane view model for a single selected group. Manages the apps list, debounced
/// composite-icon preview, and add/remove operations. <see cref="Bind"/> swaps the
/// currently edited <see cref="GroupListItemViewModel"/> so the same instance can live
/// across selection changes (avoids allocating a fresh editor per click).
/// </summary>
public sealed partial class GroupEditorViewModel : ObservableObject, IDisposable
{
    /// <summary>Delay before a composite-icon refresh fires after the last apps mutation.</summary>
    public static readonly TimeSpan PreviewDebounce = TimeSpan.FromMilliseconds(300);

    /// <summary>Pixel size of the composite preview rendered for the detail pane.</summary>
    public const int PreviewSize = 256;

    private readonly IIconExtractor _extractor;
    private readonly ICompositeIconGenerator _composer;
    private readonly IIconCache _cache;
    private readonly IGroupConfigStore _store;
    private readonly IGroupSyncService _syncService;
    private readonly IAppDataPathProvider _paths;
    private readonly ILogger<GroupEditorViewModel>? _logger;

    private GroupListItemViewModel? _boundItem;
    private CancellationTokenSource? _previewCts;
    private bool _disposed;

    /// <summary>Initializes a new instance with the dependencies it needs to render previews and persist edits.</summary>
    public GroupEditorViewModel(
        IIconExtractor extractor,
        ICompositeIconGenerator composer,
        IIconCache cache,
        IGroupConfigStore store,
        IGroupSyncService syncService,
        IAppDataPathProvider paths,
        ILogger<GroupEditorViewModel>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(extractor);
        ArgumentNullException.ThrowIfNull(composer);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(syncService);
        ArgumentNullException.ThrowIfNull(paths);

        _extractor = extractor;
        _composer = composer;
        _cache = cache;
        _store = store;
        _syncService = syncService;
        _paths = paths;
        _logger = logger;
    }

    /// <summary>Apps currently bound to the editor — empty when no group is selected.</summary>
    public ObservableCollection<AppEntryViewModel> Apps { get; } = [];

    /// <summary>Currently bound group, or null when nothing is selected.</summary>
    public GroupListItemViewModel? BoundItem
    {
        get => _boundItem;
        private set => SetProperty(ref _boundItem, value);
    }

    /// <summary>Live composite preview, regenerated on <see cref="Apps"/> changes (debounced).</summary>
    [ObservableProperty]
    private BitmapSource? _compositeIconPreview;

    /// <summary>Re-routes the editor to a new group selection.</summary>
    /// <param name="item">The new selection, or <see langword="null"/> to clear.</param>
    public void Bind(GroupListItemViewModel? item)
    {
        if (ReferenceEquals(_boundItem, item))
        {
            return;
        }

        Apps.CollectionChanged -= OnAppsCollectionChanged;
        Apps.Clear();

        BoundItem = item;
        if (item is null)
        {
            CompositeIconPreview = null;
            return;
        }

        foreach (var entry in item.Config.Apps)
        {
            var appVm = new AppEntryViewModel(entry);
            Apps.Add(appVm);
            LoadIconInto(appVm);
        }

        Apps.CollectionChanged += OnAppsCollectionChanged;
        SchedulePreviewRefresh();
    }

    [RelayCommand]
    private async Task AddAppsAsync(IEnumerable<string>? paths)
    {
        if (paths is null || _boundItem is null)
        {
            return;
        }

        var added = 0;
        foreach (var raw in paths)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            var ext = Path.GetExtension(raw);
            if (!string.Equals(ext, ".exe", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(ext, ".lnk", StringComparison.OrdinalIgnoreCase))
            {
                _logger?.LogDebug("Ignored non-executable drop {Path}", raw);
                continue;
            }

            var entry = new AppEntry
            {
                Name = Path.GetFileNameWithoutExtension(raw),
                Path = raw,
            };
            _boundItem.Config.Apps.Add(entry);

            var appVm = new AppEntryViewModel(entry);
            Apps.Add(appVm);
            LoadIconInto(appVm);
            added++;
        }

        if (added > 0)
        {
            await _store.SaveAsync(_boundItem.Config).ConfigureAwait(true);
            await _syncService.SyncAsync(_boundItem.Config).ConfigureAwait(true);
            _boundItem.NotifyAppCountChanged();
        }
    }

    [RelayCommand]
    private async Task RemoveAppAsync(AppEntryViewModel? app)
    {
        if (app is null || _boundItem is null)
        {
            return;
        }

        _boundItem.Config.Apps.Remove(app.Entry);
        Apps.Remove(app);

        await _store.SaveAsync(_boundItem.Config).ConfigureAwait(true);
        await _syncService.SyncAsync(_boundItem.Config).ConfigureAwait(true);
        _boundItem.NotifyAppCountChanged();
    }

    [RelayCommand]
    private void ShowPinHelper()
    {
        if (_boundItem is null)
        {
            return;
        }

        var shortcutPath = _paths.GetGroupShortcutFile(_boundItem.Id);
        if (!File.Exists(shortcutPath))
        {
            _logger?.LogWarning(
                "Pin-helper invoked but shortcut {Path} does not exist yet — add at least one app to generate it.",
                shortcutPath);
            return;
        }

        // Open Explorer with the .lnk pre-selected so the user can pick "Pin to taskbar"
        // (Win10 / older Win11) or use Show-More-Options on Win11 22H2+.
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"/select,\"{shortcutPath}\"",
            UseShellExecute = true,
        });
    }

    private void OnAppsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        SchedulePreviewRefresh();

    private async void SchedulePreviewRefresh()
    {
        _previewCts?.Cancel();
        _previewCts?.Dispose();
        _previewCts = new CancellationTokenSource();
        var token = _previewCts.Token;

        try
        {
            await Task.Delay(PreviewDebounce, token).ConfigureAwait(true);
            RegeneratePreview();
        }
        catch (OperationCanceledException)
        {
            // Superseded by a later change — fine.
        }
    }

    private void RegeneratePreview()
    {
        if (Apps.Count == 0)
        {
            CompositeIconPreview = null;
            return;
        }

        var icons = new List<BitmapSource>(Math.Min(Apps.Count, CompositeIconGenerator.MaxTiles));
        foreach (var app in Apps)
        {
            if (icons.Count >= CompositeIconGenerator.MaxTiles)
            {
                break;
            }

            var icon = app.Icon ?? ExtractAndCache(app.Path, PreviewSize);
            if (icon is not null)
            {
                icons.Add(icon);
            }
        }

        if (icons.Count == 0)
        {
            CompositeIconPreview = null;
            return;
        }

        CompositeIconPreview = _composer.GenerateComposite(icons, PreviewSize);
    }

    private void LoadIconInto(AppEntryViewModel app)
    {
        app.Icon = ExtractAndCache(app.Path, PreviewSize);
    }

    private BitmapSource? ExtractAndCache(string path, int size)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        if (_cache.TryGet(path, size, out var cached))
        {
            return cached;
        }

        var icon = _extractor.ExtractIcon(path, size);
        if (icon is not null)
        {
            _cache.Set(path, size, icon);
        }
        return icon;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Apps.CollectionChanged -= OnAppsCollectionChanged;
        _previewCts?.Cancel();
        _previewCts?.Dispose();
        _previewCts = null;
        _disposed = true;
    }
}
