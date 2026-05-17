using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Shared.Configuration;

/// <summary>
/// JSON-file-backed <see cref="IAppSettingsStore"/> writing to
/// <see cref="IAppDataPathProvider.SettingsFile"/>.
/// </summary>
/// <remarks>Atomic write semantics match <see cref="JsonGroupConfigStore"/>.</remarks>
public sealed class JsonAppSettingsStore : IAppSettingsStore
{
    private readonly IAppDataPathProvider _paths;
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance bound to the supplied <see cref="IAppDataPathProvider"/>.
    /// </summary>
    /// <param name="paths">Path provider for locating the settings file.</param>
    public JsonAppSettingsStore(IAppDataPathProvider paths)
    {
        ArgumentNullException.ThrowIfNull(paths);

        _paths = paths;
        _options = JsonOptions.Default;
    }

    /// <inheritdoc/>
    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_paths.SettingsFile))
        {
            return new AppSettings();
        }

        await using var stream = File.OpenRead(_paths.SettingsFile);
        var settings = await JsonSerializer.DeserializeAsync<AppSettings>(stream, _options, cancellationToken)
            .ConfigureAwait(false);

        return settings ?? new AppSettings();
    }

    /// <inheritdoc/>
    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Directory.CreateDirectory(_paths.AppDataRoot);

        var target = _paths.SettingsFile;
        var temp = target + ".tmp";

        await using (var stream = File.Create(temp))
        {
            await JsonSerializer.SerializeAsync(stream, settings, _options, cancellationToken)
                .ConfigureAwait(false);
        }

        File.Move(temp, target, overwrite: true);
    }
}
