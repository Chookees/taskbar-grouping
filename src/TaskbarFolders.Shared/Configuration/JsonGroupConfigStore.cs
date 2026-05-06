using System.IO;
using System.Text.Json;
using TaskbarFolders.Shared.Models;
using TaskbarFolders.Shared.Utilities;

namespace TaskbarFolders.Shared.Configuration;

/// <summary>
/// Stores group configurations as JSON files in the application data directory.
/// </summary>
public sealed class JsonGroupConfigStore : IGroupConfigStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <inheritdoc />
    public async Task<IReadOnlyList<GroupConfig>> LoadAllAsync()
    {
        string dir = PathHelper.GroupsDirectory;
        if (!Directory.Exists(dir))
            return [];

        var configs = new List<GroupConfig>();
        foreach (string file in Directory.GetFiles(dir, "*.json"))
        {
            GroupConfig? config = await LoadFromFileAsync(file).ConfigureAwait(false);
            if (config is not null)
                configs.Add(config);
        }

        return configs;
    }

    /// <inheritdoc />
    public async Task<GroupConfig?> LoadAsync(string groupId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);

        string filePath = PathHelper.GetGroupFilePath(groupId);
        if (!File.Exists(filePath))
            return null;

        return await LoadFromFileAsync(filePath).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SaveAsync(GroupConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        Directory.CreateDirectory(PathHelper.GroupsDirectory);

        string filePath = PathHelper.GetGroupFilePath(config.Id);
        string json = JsonSerializer.Serialize(config, JsonOptions);
        await File.WriteAllTextAsync(filePath, json).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task DeleteAsync(string groupId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);

        string filePath = PathHelper.GetGroupFilePath(groupId);
        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    private static async Task<GroupConfig?> LoadFromFileAsync(string filePath)
    {
        try
        {
            string json = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
            return JsonSerializer.Deserialize<GroupConfig>(json, JsonOptions);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
