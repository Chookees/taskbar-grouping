using System.Windows.Media.Imaging;
using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Launcher.ViewModels;

/// <summary>
/// ViewModel wrapping an AppEntry for display in the popup grid.
/// </summary>
public sealed class AppEntryViewModel : ViewModelBase
{
    private BitmapSource? _icon;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppEntryViewModel"/> class.
    /// </summary>
    /// <param name="model">The underlying AppEntry model.</param>
    public AppEntryViewModel(AppEntry model)
    {
        ArgumentNullException.ThrowIfNull(model);
        Model = model;
    }

    /// <summary>
    /// The underlying model.
    /// </summary>
    public AppEntry Model { get; }

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name => Model.Name;

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
