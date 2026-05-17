namespace TaskbarFolders.Manager.Services;

/// <summary>
/// Reads the current Windows app-theme preference. Abstracted from the registry-backed
/// implementation so tests can swap in a deterministic value without touching HKCU.
/// </summary>
public interface ISystemThemeProbe
{
    /// <summary>Returns <see langword="true"/> when Windows is configured for light apps.</summary>
    bool IsLightMode { get; }
}
