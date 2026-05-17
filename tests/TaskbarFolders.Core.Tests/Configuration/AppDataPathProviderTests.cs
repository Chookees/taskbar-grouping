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
}
