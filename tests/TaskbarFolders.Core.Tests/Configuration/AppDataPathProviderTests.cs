using System.IO;
using FluentAssertions;
using TaskbarFolders.Shared.Configuration;
using Xunit;

namespace TaskbarFolders.Core.Tests.Configuration;

public class AppDataPathProviderTests
{
    [Fact]
    public void AppDataRoot_AppendsTaskbarFoldersToBaseDirectory()
    {
        var sut = new AppDataPathProvider("C:/some/base");

        sut.AppDataRoot.Should().Be(Path.Combine("C:/some/base", "TaskbarFolders"));
    }

    [Fact]
    public void GroupsDirectory_IsRootSlashGroups()
    {
        var sut = new AppDataPathProvider("C:/base");

        sut.GroupsDirectory.Should().Be(Path.Combine("C:/base", "TaskbarFolders", "groups"));
    }

    [Fact]
    public void IconsDirectory_IsRootSlashIcons()
    {
        var sut = new AppDataPathProvider("C:/base");

        sut.IconsDirectory.Should().Be(Path.Combine("C:/base", "TaskbarFolders", "icons"));
    }

    [Fact]
    public void LogsDirectory_IsRootSlashLogs()
    {
        var sut = new AppDataPathProvider("C:/base");

        sut.LogsDirectory.Should().Be(Path.Combine("C:/base", "TaskbarFolders", "logs"));
    }

    [Fact]
    public void ShortcutsDirectory_IsRootSlashShortcuts()
    {
        var sut = new AppDataPathProvider("C:/base");

        sut.ShortcutsDirectory.Should().Be(Path.Combine("C:/base", "TaskbarFolders", "shortcuts"));
    }

    [Fact]
    public void SettingsFile_IsSettingsJsonAtRoot()
    {
        var sut = new AppDataPathProvider("C:/base");

        sut.SettingsFile.Should().Be(Path.Combine("C:/base", "TaskbarFolders", "settings.json"));
    }

    [Fact]
    public void GetGroupFile_AppendsIdJsonToGroupsDirectory()
    {
        var sut = new AppDataPathProvider("C:/base");

        sut.GetGroupFile("abc")
            .Should().Be(Path.Combine("C:/base", "TaskbarFolders", "groups", "abc.json"));
    }

    [Fact]
    public void GetGroupIconFile_AppendsIdIcoToIconsDirectory()
    {
        var sut = new AppDataPathProvider("C:/base");

        sut.GetGroupIconFile("abc")
            .Should().Be(Path.Combine("C:/base", "TaskbarFolders", "icons", "abc.ico"));
    }

    [Fact]
    public void GetGroupShortcutFile_AppendsIdLnkToShortcutsDirectory()
    {
        var sut = new AppDataPathProvider("C:/base");

        sut.GetGroupShortcutFile("abc")
            .Should().Be(Path.Combine("C:/base", "TaskbarFolders", "shortcuts", "abc.lnk"));
    }

    [Theory]
    [InlineData("../escape")]
    [InlineData("..\\escape")]
    [InlineData("with/slash")]
    [InlineData("with\\backslash")]
    [InlineData("with space")]
    [InlineData("name:colon")]
    [InlineData("name*star")]
    [InlineData("")]
    [InlineData("  ")]
    public void GetGroup_RejectsIdsThatCouldEscapeTheAppDataRoot(string badId)
    {
        var sut = new AppDataPathProvider("C:/base");

        FluentActions.Invoking(() => sut.GetGroupFile(badId)).Should().Throw<System.ArgumentException>();
        FluentActions.Invoking(() => sut.GetGroupIconFile(badId)).Should().Throw<System.ArgumentException>();
        FluentActions.Invoking(() => sut.GetGroupShortcutFile(badId)).Should().Throw<System.ArgumentException>();
    }

    [Theory]
    [InlineData("guid-like_id.1")]
    [InlineData("550e8400e29b41d4a716446655440000")]
    [InlineData("with-hyphen")]
    [InlineData("with.dot")]
    [InlineData("with_underscore")]
    public void GetGroup_AcceptsValidIdShapes(string okId)
    {
        var sut = new AppDataPathProvider("C:/base");

        // Should not throw for any of these.
        sut.GetGroupFile(okId);
        sut.GetGroupIconFile(okId);
        sut.GetGroupShortcutFile(okId);
    }
}
