using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TaskbarFolders.Shared.Configuration;

/// <summary>
/// <see cref="JsonStringEnumConverter{TEnum}"/> pre-configured to emit camelCase names
/// (so JSON stays consistent with the rest of the document where property names are camelCased).
/// Reads remain case-insensitive — existing configs with "system"/"System"/"SYSTEM" all work.
/// </summary>
/// <typeparam name="TEnum">The enum type being converted.</typeparam>
public sealed class CamelCaseEnumConverter<TEnum> : JsonStringEnumConverter<TEnum>
    where TEnum : struct, Enum
{
    /// <summary>Initializes a new converter writing values in camelCase.</summary>
    public CamelCaseEnumConverter()
        : base(JsonNamingPolicy.CamelCase, allowIntegerValues: false)
    {
    }
}
