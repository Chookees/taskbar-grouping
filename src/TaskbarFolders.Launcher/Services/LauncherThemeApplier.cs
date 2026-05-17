using System;
using System.Runtime.Versioning;
using System.Windows;
using Microsoft.Win32;
using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Launcher.Services;

/// <summary>
/// One-shot theme applier for the launcher process. The launcher window opens, lives
/// briefly, and closes — there is no need for a live theme-change listener like the
/// Manager has, so this is a plain static helper rather than a hosted service.
/// </summary>
[SupportedOSPlatform("windows")]
public static class LauncherThemeApplier
{
    private const string LightDictionaryUri = "/TaskbarFolders.Launcher;component/Themes/Light.xaml";
    private const string DarkDictionaryUri = "/TaskbarFolders.Launcher;component/Themes/Dark.xaml";

    private const string PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string AppsUseLightThemeValue = "AppsUseLightTheme";

    /// <summary>Merges the chosen theme dictionary into <see cref="Application.Resources"/>.</summary>
    public static void Apply(Application app, ThemePreference preference)
    {
        ArgumentNullException.ThrowIfNull(app);

        var effective = Resolve(preference);
        var uri = effective == ThemePreference.Dark
            ? new Uri(DarkDictionaryUri, UriKind.Relative)
            : new Uri(LightDictionaryUri, UriKind.Relative);

        app.Resources.MergedDictionaries.Insert(0, new ResourceDictionary { Source = uri });
    }

    /// <summary>
    /// Resolves <see cref="ThemePreference.System"/> against the current Windows setting.
    /// Light/Dark return verbatim. Defaults to Light if the registry key is missing.
    /// </summary>
    public static ThemePreference Resolve(ThemePreference preference)
    {
        if (preference != ThemePreference.System)
        {
            return preference;
        }

        using var key = Registry.CurrentUser.OpenSubKey(PersonalizeKey, writable: false);
        return key?.GetValue(AppsUseLightThemeValue) is int value && value == 0
            ? ThemePreference.Dark
            : ThemePreference.Light;
    }
}
