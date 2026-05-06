using System.IO;
using System.Text.Json;
using TaskbarFolders.Shared.Models;
using TaskbarFolders.Shared.Utilities;

namespace TaskbarFolders.Shared.Configuration;

/// <summary>
/// Stores application settings as a JSON file in the application data directory.
/// </summary>
public sealed class JsonAppSettingsStore : IAppSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <inheritdoc />
    public async Task<AppSettings> LoadAsync()
    {
        string filePath = PathHelper.SettingsFilePath;
        if (!File.Exists(filePath))
            return new AppSettings();

        try
        {
            string json = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
            return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
        }
        catch (Exception)
        {
            return new AppSettings();
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        string? directory = Path.GetDirectoryName(PathHelper.SettingsFilePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        string json = JsonSerializer.Serialize(settings, JsonOptions);
        await File.WriteAllTextAsync(PathHelper.SettingsFilePath, json).ConfigureAwait(false);
    }
}
