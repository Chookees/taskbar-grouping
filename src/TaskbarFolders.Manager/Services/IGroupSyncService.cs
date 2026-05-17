using System.Threading;
using System.Threading.Tasks;
using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Manager.Services;

/// <summary>
/// Keeps the on-disk per-group artifacts (composite .ico + pinnable .lnk) in sync with the
/// current <see cref="GroupConfig"/>. Called whenever the Manager persists a group.
/// </summary>
public interface IGroupSyncService
{
    /// <summary>
    /// Regenerates the composite icon and the per-group shortcut. No-op when the group has
    /// zero apps — the shortcut is left untouched so a pinned tile does not silently lose
    /// its icon while the user is half-way through editing.
    /// </summary>
    Task SyncAsync(GroupConfig config, CancellationToken cancellationToken = default);

    /// <summary>Removes the per-group icon and shortcut files. Silent on missing files.</summary>
    void RemoveArtifacts(string groupId);
}
