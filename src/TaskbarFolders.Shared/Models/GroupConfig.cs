using System.Text.Json.Serialization;

namespace TaskbarFolders.Shared.Models;

/// <summary>
/// Configuration for a single taskbar group.
/// </summary>
public sealed class GroupConfig
{
    /// <summary>
    /// Unique identifier for the group.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Display name of the group.
    /// </summary>
    [JsonPropertyName("groupName")]
    public required string GroupName { get; set; }

    /// <summary>
    /// Number of columns in the popup grid layout.
    /// </summary>
    [JsonPropertyName("columns")]
    public int Columns { get; set; } = 3;

    /// <summary>
    /// Theme override for this group (light, dark, or system).
    /// </summary>
    [JsonPropertyName("theme")]
    public string Theme { get; set; } = "system";

    /// <summary>
    /// Applications contained in this group.
    /// </summary>
    [JsonPropertyName("apps")]
    public List<AppEntry> Apps { get; set; } = [];
}
