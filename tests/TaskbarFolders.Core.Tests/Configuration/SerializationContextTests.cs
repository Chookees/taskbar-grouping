using System.Collections.Generic;
using FluentAssertions;
using TaskbarFolders.Shared.Configuration;
using TaskbarFolders.Shared.Models;
using Xunit;

namespace TaskbarFolders.Core.Tests.Configuration;

public class SerializationContextTests
{
    [Fact]
    public void Default_ExposesGroupConfig()
    {
        SerializationContext.Default.GroupConfig.Should().NotBeNull();
    }

    [Fact]
    public void Default_ExposesAppSettings()
    {
        SerializationContext.Default.AppSettings.Should().NotBeNull();
    }

    [Fact]
    public void Default_ExposesAppEntry()
    {
        SerializationContext.Default.AppEntry.Should().NotBeNull();
    }

    [Fact]
    public void Default_ExposesListOfGroupConfig()
    {
        SerializationContext.Default.ListGroupConfig.Should().NotBeNull();
    }

    [Fact]
    public void JsonOptions_UseSourceGeneratedResolver()
    {
        JsonOptions.Default.TypeInfoResolver.Should().BeSameAs(SerializationContext.Default);
    }

    [Fact]
    public void JsonOptions_ProducesSerializableInstance_ForKnownTypes()
    {
        // Smoke check that the resolver chain actually serialises our domain models.
        var config = new GroupConfig { Id = "abc", GroupName = "Tools" };

        var json = System.Text.Json.JsonSerializer.Serialize(config, JsonOptions.Default);

        json.Should().Contain("\"id\": \"abc\"");
        json.Should().Contain("\"groupName\": \"Tools\"");
    }

    [Fact]
    public void JsonOptions_RoundTripsAllListedTypes()
    {
        var input = new List<GroupConfig>
        {
            new() { Id = "1", GroupName = "A" },
            new() { Id = "2", GroupName = "B" },
        };

        var json = System.Text.Json.JsonSerializer.Serialize(input, JsonOptions.Default);
        var roundtrip = System.Text.Json.JsonSerializer.Deserialize<List<GroupConfig>>(json, JsonOptions.Default);

        roundtrip.Should().NotBeNull();
        roundtrip!.Should().HaveCount(2);
        roundtrip![0].Id.Should().Be("1");
        roundtrip[1].GroupName.Should().Be("B");
    }
}
