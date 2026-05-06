using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Shared.Configuration;

/// <summary>
/// Persists and retrieves group configurations.
/// </summary>
public interface IGroupConfigStore
{
    /// <summary>
    /// Loads all saved group configurations.
    /// </summary>
    Task<IReadOnlyList<GroupConfig>> LoadAllAsync();

    /// <summary>
    /// Loads a single group configuration by its ID.
    /// </summary>
    /// <param name="groupId">The unique group identifier.</param>
    Task<GroupConfig?> LoadAsync(string groupId);

    /// <summary>
    /// Saves a group configuration. Creates or overwrites the file.
    /// </summary>
    /// <param name="config">The group configuration to save.</param>
    Task SaveAsync(GroupConfig config);

    /// <summary>
    /// Deletes a group configuration by its ID.
    /// </summary>
    /// <param name="groupId">The unique group identifier.</param>
    Task DeleteAsync(string groupId);
}
