using System.Text.Json.Serialization;

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
    /// Global theme setting (light, dark, or system).
    /// </summary>
    [JsonPropertyName("theme")]
    public string Theme { get; set; } = "system";

    /// <summary>
    /// Whether to enable popup animations.
    /// </summary>
    [JsonPropertyName("enableAnimations")]
    public bool EnableAnimations { get; set; } = true;

    /// <summary>
    /// Popup position preference (auto, above, below).
    /// </summary>
    [JsonPropertyName("popupPosition")]
    public string PopupPosition { get; set; } = "auto";
}
