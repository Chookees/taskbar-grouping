using System;
using CommunityToolkit.Mvvm.ComponentModel;
using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Manager.ViewModels;

/// <summary>
/// Sidebar entry representing a single <see cref="GroupConfig"/>. Mutating <see cref="Name"/>
/// writes back to the wrapped config so changes propagate when the host view model saves.
/// </summary>
public sealed partial class GroupListItemViewModel : ObservableObject
{
    /// <summary>Initializes a new wrapper around an existing config.</summary>
    /// <param name="config">Underlying configuration; identity is shared by reference.</param>
    public GroupListItemViewModel(GroupConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        Config = config;
        _name = config.GroupName;
    }

    /// <summary>Underlying configuration. Visible so the editor view model can mutate it directly.</summary>
    public GroupConfig Config { get; }

    /// <summary>Stable identifier delegating to <see cref="Config"/>.</summary>
    public string Id => Config.Id;

    /// <summary>Display name. Two-way bound; assignment keeps <see cref="Config"/> in sync.</summary>
    [ObservableProperty]
    private string _name;

    partial void OnNameChanged(string value)
    {
        Config.GroupName = value;
    }

    /// <summary>Convenience pass-through for sidebar item count badges.</summary>
    public int AppCount => Config.Apps.Count;

    /// <summary>
    /// Raises <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/> for
    /// <see cref="AppCount"/>. Called by the editor after it mutates <see cref="Config"/>.Apps
    /// so the sidebar badge refreshes.
    /// </summary>
    public void NotifyAppCountChanged() => OnPropertyChanged(nameof(AppCount));
}
