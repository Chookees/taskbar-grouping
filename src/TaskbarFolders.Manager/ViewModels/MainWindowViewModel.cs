using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using TaskbarFolders.Shared.Configuration;
using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Manager.ViewModels;

/// <summary>
/// Hosts the sidebar group list and the commands that mutate it. Persistence is delegated
/// to <see cref="IGroupConfigStore"/> so the view model is fully test-friendly.
/// </summary>
public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly IGroupConfigStore _store;
    private readonly ILogger<MainWindowViewModel>? _logger;

    /// <summary>Initializes a new instance.</summary>
    /// <param name="store">Group configuration store, injected.</param>
    /// <param name="logger">Optional logger.</param>
    public MainWindowViewModel(IGroupConfigStore store, ILogger<MainWindowViewModel>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(store);

        _store = store;
        _logger = logger;
    }

    /// <summary>Window title shown in the chrome.</summary>
    [ObservableProperty]
    private string _title = "TaskbarFolders Manager";

    /// <summary>Pending name for <see cref="AddGroupCommand"/>; bound to the sidebar TextBox.</summary>
    [ObservableProperty]
    private string _newGroupName = string.Empty;

    /// <summary>Currently highlighted sidebar entry, or <see langword="null"/> if none.</summary>
    [ObservableProperty]
    private GroupListItemViewModel? _selectedGroup;

    /// <summary>Sidebar items in alphabetical order by <see cref="GroupListItemViewModel.Name"/>.</summary>
    public ObservableCollection<GroupListItemViewModel> Groups { get; } = [];

    /// <summary>
    /// Replaces <see cref="Groups"/> with the contents of the store. Call once after construction
    /// (App bootstrap or window Loaded event).
    /// </summary>
    public async Task LoadGroupsAsync(CancellationToken cancellationToken = default)
    {
        var configs = await _store.LoadAllAsync(cancellationToken).ConfigureAwait(true);

        Groups.Clear();
        foreach (var config in configs.OrderBy(g => g.GroupName, StringComparer.CurrentCultureIgnoreCase))
        {
            Groups.Add(new GroupListItemViewModel(config));
        }

        _logger?.LogInformation("Loaded {Count} group(s) from store.", Groups.Count);
    }

    [RelayCommand]
    private async Task AddGroupAsync()
    {
        var trimmed = NewGroupName?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return;
        }

        var config = new GroupConfig { GroupName = trimmed };
        await _store.SaveAsync(config).ConfigureAwait(true);

        var item = new GroupListItemViewModel(config);
        InsertAlphabetically(item);
        SelectedGroup = item;
        NewGroupName = string.Empty;

        _logger?.LogInformation("Created group {Id} '{Name}'.", config.Id, config.GroupName);
    }

    [RelayCommand]
    private async Task DeleteGroupAsync(GroupListItemViewModel? group)
    {
        if (group is null)
        {
            return;
        }

        await _store.DeleteAsync(group.Id).ConfigureAwait(true);

        var index = Groups.IndexOf(group);
        Groups.Remove(group);

        if (ReferenceEquals(SelectedGroup, group))
        {
            // Move selection to the neighbour at the same index, if any.
            SelectedGroup = Groups.Count == 0
                ? null
                : Groups[Math.Min(index, Groups.Count - 1)];
        }

        _logger?.LogInformation("Deleted group {Id}.", group.Id);
    }

    private void InsertAlphabetically(GroupListItemViewModel item)
    {
        var index = 0;
        while (index < Groups.Count &&
               StringComparer.CurrentCultureIgnoreCase.Compare(Groups[index].Name, item.Name) < 0)
        {
            index++;
        }
        Groups.Insert(index, item);
    }
}
