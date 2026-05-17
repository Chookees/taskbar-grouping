using System.Collections.Generic;
using System.Text.Json.Serialization;
using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Shared.Configuration;

/// <summary>
/// Source-generated <see cref="JsonSerializerContext"/> for the persistence models.
/// Eliminates reflection on the hot serialise/deserialise paths and is a prerequisite
/// for any future trimming or AOT publish profile.
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(GroupConfig))]
[JsonSerializable(typeof(AppSettings))]
[JsonSerializable(typeof(AppEntry))]
[JsonSerializable(typeof(List<GroupConfig>))]
public sealed partial class SerializationContext : JsonSerializerContext;
