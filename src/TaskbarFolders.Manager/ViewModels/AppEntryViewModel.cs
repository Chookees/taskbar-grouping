using System;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Manager.ViewModels;

/// <summary>
/// Row in the editor's app list — wraps a single <see cref="AppEntry"/> and carries the
/// extracted icon for display. Two-way bindings on <see cref="Name"/>, <see cref="Path"/>,
/// and <see cref="Arguments"/> write through to the underlying entry so saves see edits.
/// <see cref="DisplayPath"/> is the presentation form of <see cref="Path"/>; the entry always
/// stores and launches the real one.
/// </summary>
public sealed partial class AppEntryViewModel : ObservableObject
{
    /// <summary>Initializes a wrapper around an existing app entry.</summary>
    /// <param name="entry">Underlying app entry; identity is shared by reference.</param>
    public AppEntryViewModel(AppEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        Entry = entry;
        _name = entry.Name;
        _path = entry.Path;
        _arguments = entry.Arguments;
    }

    /// <summary>Underlying entry. Public so the editor view model can persist it.</summary>
    public AppEntry Entry { get; }

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayPath))]
    private string _path;

    /// <summary>
    /// <see cref="Path"/> with the user's profile directory collapsed to <c>%USERPROFILE%</c>,
    /// for display only. The row's tooltip carries the real path.
    /// </summary>
    public string DisplayPath => PathDisplay.ForDisplay(Path);

    [ObservableProperty]
    private string? _arguments;

    /// <summary>Icon extracted from <see cref="Path"/>; set asynchronously by the editor.</summary>
    [ObservableProperty]
    private BitmapSource? _icon;

    partial void OnNameChanged(string value) => Entry.Name = value;
    partial void OnPathChanged(string value) => Entry.Path = value;
    partial void OnArgumentsChanged(string? value) => Entry.Arguments = value;
}
