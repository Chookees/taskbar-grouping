using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Shared.Configuration;

/// <summary>
/// Persists and retrieves <see cref="GroupConfig"/> documents.
/// Implementations are expected to be safe for single-writer / multi-reader access.
/// </summary>
public interface IGroupConfigStore
{
    /// <summary>Loads every group present in the store.</summary>
    Task<IReadOnlyList<GroupConfig>> LoadAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Loads a single group by identifier, or <see langword="null"/> if no such group exists.</summary>
    Task<GroupConfig?> LoadAsync(string groupId, CancellationToken cancellationToken = default);

    /// <summary>Creates or overwrites the document for the supplied group.</summary>
    Task SaveAsync(GroupConfig config, CancellationToken cancellationToken = default);

    /// <summary>Removes the document for the supplied group. No-op if absent.</summary>
    Task DeleteAsync(string groupId, CancellationToken cancellationToken = default);
}
