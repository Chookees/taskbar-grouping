using System.Runtime.Versioning;
using Microsoft.Win32;

namespace TaskbarFolders.Manager.Services;

/// <summary>
/// Default <see cref="ISystemThemeProbe"/> reading
/// <c>HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize\AppsUseLightTheme</c>.
/// Missing key or value defaults to light mode (matches a fresh Windows install).
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class RegistrySystemThemeProbe : ISystemThemeProbe
{
    /// <summary>Sub-key under <see cref="Registry.CurrentUser"/> containing personalization settings.</summary>
    public const string PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    /// <summary>Registry value name. <c>0</c> = dark, <c>1</c> = light.</summary>
    public const string AppsUseLightThemeValueName = "AppsUseLightTheme";

    /// <inheritdoc/>
    public bool IsLightMode
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(PersonalizeKey, writable: false);
            return key?.GetValue(AppsUseLightThemeValueName) is not int value || value != 0;
        }
    }
}
