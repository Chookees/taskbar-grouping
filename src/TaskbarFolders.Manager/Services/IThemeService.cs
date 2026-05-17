using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Manager.Services;

/// <summary>
/// Applies the user-selected theme to the application resources and exposes
/// the effective theme (resolving <see cref="ThemePreference.System"/> against the
/// current Windows configuration).
/// </summary>
public interface IThemeService
{
    /// <summary>Current user preference.</summary>
    ThemePreference Preference { get; }

    /// <summary>
    /// Resolved theme actually applied to the UI — equals <see cref="Preference"/> for
    /// Light/Dark, and the current Windows theme for <see cref="ThemePreference.System"/>.
    /// </summary>
    ThemePreference EffectiveTheme { get; }

    /// <summary>
    /// Replaces the current preference, swaps the active <see cref="System.Windows.ResourceDictionary"/>
    /// in <see cref="System.Windows.Application.Resources"/>, and starts/stops the OS-theme
    /// listener as appropriate.
    /// </summary>
    void SetPreference(ThemePreference preference);
}
