using FluentAssertions;
using TaskbarFolders.Launcher.Configuration;
using Xunit;

namespace TaskbarFolders.Launcher.Tests;

public class CommandLineParserTests
{
    [Fact]
    public void TryParseGroupId_ReturnsValue_WhenArgumentPresent()
    {
        var args = new[] { "--group-id", "abc-123" };

        CommandLineParser.TryParseGroupId(args).Should().Be("abc-123");
    }

    [Fact]
    public void TryParseGroupId_IsCaseInsensitive_ForArgumentName()
    {
        var args = new[] { "--Group-Id", "value" };

        CommandLineParser.TryParseGroupId(args).Should().Be("value");
    }

    [Fact]
    public void TryParseGroupId_ReturnsNull_WhenArgumentMissing()
    {
        var args = new[] { "--other", "value" };

        CommandLineParser.TryParseGroupId(args).Should().BeNull();
    }

    [Fact]
    public void TryParseGroupId_ReturnsNull_WhenArgumentIsLast()
    {
        // No value following the flag — must not crash with IndexOutOfRange.
        var args = new[] { "--group-id" };

        CommandLineParser.TryParseGroupId(args).Should().BeNull();
    }

    [Fact]
    public void TryParseGroupId_ReturnsNull_WhenArrayEmpty()
    {
        CommandLineParser.TryParseGroupId(System.Array.Empty<string>()).Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void TryParseGroupId_ReturnsNull_ForBlankValue(string blank)
    {
        var args = new[] { "--group-id", blank };

        CommandLineParser.TryParseGroupId(args).Should().BeNull();
    }
}
