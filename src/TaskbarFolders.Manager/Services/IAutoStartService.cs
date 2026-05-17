namespace TaskbarFolders.Manager.Services;

/// <summary>
/// Controls whether the Manager launches automatically with Windows.
/// Implementations write to a per-user run target — no elevation required.
/// </summary>
public interface IAutoStartService
{
    /// <summary>Returns <see langword="true"/> if the auto-start entry is currently registered.</summary>
    bool IsEnabled { get; }

    /// <summary>Registers the auto-start entry. No-op if already enabled.</summary>
    void Enable();

    /// <summary>Removes the auto-start entry. No-op if absent.</summary>
    void Disable();
}
