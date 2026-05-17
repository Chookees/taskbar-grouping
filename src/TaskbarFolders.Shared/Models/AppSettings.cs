using System.Text.Json.Serialization;
using TaskbarFolders.Shared.Configuration;

namespace TaskbarFolders.Shared.Models;

/// <summary>
/// Global application settings for TaskbarFolders.
/// </summary>
public sealed class AppSettings
{
    /// <summary>
    /// Whether to start the Manager application on Windows startup.
    /// </summary>
    [JsonPropertyName("autoStart")]
    public bool AutoStart { get; set; }

    /// <summary>
    /// Global theme setting.
    /// </summary>
    [JsonPropertyName("theme")]
    [JsonConverter(typeof(CamelCaseEnumConverter<ThemePreference>))]
    public ThemePreference Theme { get; set; } = ThemePreference.System;

    /// <summary>
    /// Whether to enable popup animations.
    /// </summary>
    [JsonPropertyName("enableAnimations")]
    public bool EnableAnimations { get; set; } = true;

    /// <summary>
    /// Popup position preference.
    /// </summary>
    [JsonPropertyName("popupPosition")]
    [JsonConverter(typeof(CamelCaseEnumConverter<PopupPositionPreference>))]
    public PopupPositionPreference PopupPosition { get; set; } = PopupPositionPreference.Auto;
}
