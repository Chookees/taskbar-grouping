using System.Threading;
using System.Threading.Tasks;
using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Shared.Configuration;

/// <summary>
/// Persists and retrieves the global <see cref="AppSettings"/> document.
/// </summary>
public interface IAppSettingsStore
{
    /// <summary>
    /// Loads the global settings. Returns a freshly defaulted <see cref="AppSettings"/>
    /// when no document has been stored yet.
    /// </summary>
    Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>Creates or overwrites the global settings document.</summary>
    Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default);
}
