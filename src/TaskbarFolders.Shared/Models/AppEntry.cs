using System.Text.Json.Serialization;

namespace TaskbarFolders.Shared.Models;

/// <summary>
/// Represents a single application entry within a taskbar group.
/// </summary>
public sealed class AppEntry
{
    /// <summary>
    /// Display name of the application.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Full path to the executable or shortcut file.
    /// </summary>
    [JsonPropertyName("path")]
    public required string Path { get; set; }

    /// <summary>
    /// Optional custom icon path. If null, the icon is extracted from the executable.
    /// </summary>
    [JsonPropertyName("iconPath")]
    public string? IconPath { get; set; }

    /// <summary>
    /// Optional command-line arguments to pass when launching the application.
    /// </summary>
    [JsonPropertyName("arguments")]
    public string Arguments { get; set; } = string.Empty;
}
