using System.Text.Json;
using System.Text.Json.Serialization;

namespace TaskbarFolders.Shared.Configuration;

/// <summary>
/// Centralised <see cref="JsonSerializerOptions"/> profile used by every persistence component.
/// Keeping this single-sourced prevents accidental drift in casing or null-handling between modules.
/// </summary>
public static class JsonOptions
{
    /// <summary>
    /// Default options: camelCase property names, indented output, null values omitted on write,
    /// and source-generated type metadata via <see cref="SerializationContext"/>.
    /// </summary>
    public static JsonSerializerOptions Default { get; } = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        TypeInfoResolver = SerializationContext.Default,
    };
}
