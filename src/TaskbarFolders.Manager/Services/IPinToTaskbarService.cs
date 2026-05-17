using System.Threading;
using System.Threading.Tasks;

namespace TaskbarFolders.Manager.Services;

/// <summary>
/// Asks Windows to pin a group's shortcut to the taskbar. Backed by the Launcher process
/// running in --pin-mode, which invokes <c>Windows.UI.Shell.TaskbarManager.RequestPinCurrentAppAsync</c>.
/// </summary>
public interface IPinToTaskbarService
{
    /// <summary>
    /// Spawns the launcher in pin-mode for the given group and awaits the result. The
    /// system displays a native "Allow [App] to pin?" dialog; the user clicks Allow or
    /// Cancel.
    /// </summary>
    Task<PinResult> PinAsync(string groupId, CancellationToken cancellationToken = default);
}

/// <summary>Outcome of an <see cref="IPinToTaskbarService.PinAsync"/> call.</summary>
public enum PinResult
{
    /// <summary>Group was pinned to the taskbar.</summary>
    Success,

    /// <summary>User clicked Cancel in the system dialog.</summary>
    UserDenied,

    /// <summary>
    /// TaskbarManager not available on this Windows build or pinning policy disabled
    /// (LTSC, Education, restricted Group Policy). Caller should fall back to the manual
    /// Explorer / .lnk flow.
    /// </summary>
    Unsupported,

    /// <summary>Spawning the launcher failed, the process timed out, or it crashed.</summary>
    Error,
}
