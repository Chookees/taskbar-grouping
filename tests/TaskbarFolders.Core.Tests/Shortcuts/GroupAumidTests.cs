using System;
using FluentAssertions;
using TaskbarFolders.Core.Shortcuts;
using Xunit;

namespace TaskbarFolders.Core.Tests.Shortcuts;

public class GroupAumidTests
{
    [Theory]
    [InlineData("g1", "TaskbarFolders.Group.g1")]
    [InlineData("550e8400-e29b-41d4-a716-446655440000", "TaskbarFolders.Group.550e8400-e29b-41d4-a716-446655440000")]
    public void For_PrefixesGroupIdWithStableNamespace(string groupId, string expected)
    {
        GroupAumid.For(groupId).Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void For_RejectsBlankGroupId(string? groupId)
    {
        var act = () => GroupAumid.For(groupId!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Prefix_IsDocumentedConstant()
    {
        GroupAumid.Prefix.Should().Be("TaskbarFolders.Group.");
    }

    [Theory]
    [InlineData("g1")]
    [InlineData("550e8400-e29b-41d4-a716-446655440000")]
    [InlineData("3173c18755824bd0a107fc0c9cd78859")]
    public void TryExtractGroupId_RoundTripsWithFor(string groupId)
    {
        var aumid = GroupAumid.For(groupId);

        GroupAumid.TryExtractGroupId(aumid, out var parsed).Should().BeTrue();
        parsed.Should().Be(groupId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("SomeOtherApp.Foo.bar")]
    [InlineData("TaskbarFolders.Group.")]
    [InlineData("TaskbarFolders.Group.   ")]
    [InlineData("taskbarfolders.group.x")] // case-sensitive prefix check; must reject lower-case
    public void TryExtractGroupId_RejectsMalformedOrForeignAumid(string? aumid)
    {
        GroupAumid.TryExtractGroupId(aumid, out var parsed).Should().BeFalse();
        parsed.Should().BeEmpty();
    }
}
