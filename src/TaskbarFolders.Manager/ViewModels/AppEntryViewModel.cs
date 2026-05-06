using System.Windows.Media.Imaging;
using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Manager.ViewModels;

/// <summary>
/// ViewModel wrapping an AppEntry for display in the group editor.
/// </summary>
public sealed class AppEntryViewModel : ViewModelBase
{
    private BitmapSource? _icon;
    private string _name;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppEntryViewModel"/> class.
    /// </summary>
    public AppEntryViewModel(AppEntry model)
    {
        ArgumentNullException.ThrowIfNull(model);
        Model = model;
        _name = model.Name;
    }

    /// <summary>
    /// The underlying model.
    /// </summary>
    public AppEntry Model { get; }

    /// <summary>
    /// Display name (editable).
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
                Model.Name = value;
        }
    }

    /// <summary>
    /// Path to the executable.
    /// </summary>
    public string Path => Model.Path;

    /// <summary>
    /// The extracted icon for display.
    /// </summary>
    public BitmapSource? Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value);
    }
}
