using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Shared.Configuration;

/// <summary>
/// Persists and retrieves global application settings.
/// </summary>
public interface IAppSettingsStore
{
    /// <summary>
    /// Loads the application settings, returning defaults if no settings file exists.
    /// </summary>
    Task<AppSettings> LoadAsync();

    /// <summary>
    /// Saves the application settings.
    /// </summary>
    /// <param name="settings">The settings to save.</param>
    Task SaveAsync(AppSettings settings);
}
