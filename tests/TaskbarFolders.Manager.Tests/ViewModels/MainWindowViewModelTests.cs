using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TaskbarFolders.Core.Icons;
using TaskbarFolders.Manager.Services;
using TaskbarFolders.Manager.ViewModels;
using TaskbarFolders.Shared.Configuration;
using TaskbarFolders.Shared.Models;
using Xunit;

namespace TaskbarFolders.Manager.Tests.ViewModels;

public class MainWindowViewModelTests
{
    private static Mock<IGroupConfigStore> CreateStoreWith(params GroupConfig[] groups)
    {
        var mock = new Mock<IGroupConfigStore>(MockBehavior.Strict);
        mock.Setup(s => s.LoadAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<GroupConfig>)groups);
        mock.Setup(s => s.SaveAsync(It.IsAny<GroupConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    private static GroupEditorViewModel CreateEditor(IGroupConfigStore store)
    {
        return new GroupEditorViewModel(
            Mock.Of<IIconExtractor>(),
            Mock.Of<ICompositeIconGenerator>(),
            Mock.Of<IIconCache>(),
            store,
            Mock.Of<IGroupSyncService>(),
            Mock.Of<IAppDataPathProvider>());
    }

    private static MainWindowViewModel CreateSut(Mock<IGroupConfigStore> store) =>
        new(store.Object, CreateEditor(store.Object), Mock.Of<IGroupSyncService>());

    [Fact]
    public void Title_HasDefaultValue()
    {
        var store = CreateStoreWith();
        var sut = CreateSut(store);
        sut.Title.Should().Be("TaskbarFolders Manager");
    }

    [Fact]
    public async Task LoadGroupsAsync_PopulatesGroupsInAlphabeticalOrder()
    {
        var store = CreateStoreWith(
            new GroupConfig { Id = "b", GroupName = "Browsers" },
            new GroupConfig { Id = "a", GroupName = "Apps" },
            new GroupConfig { Id = "c", GroupName = "code" });

        var sut = CreateSut(store);
        await sut.LoadGroupsAsync();

        sut.Groups.Select(g => g.Name).Should().Equal("Apps", "Browsers", "code");
    }

    [Fact]
    public async Task LoadGroupsAsync_ReplacesPreviousContents()
    {
        var store = CreateStoreWith(new GroupConfig { Id = "x", GroupName = "X" });
        var sut = CreateSut(store);
        await sut.LoadGroupsAsync();
        sut.Groups.Should().HaveCount(1);

        // Second load with fresh data — old items must not linger.
        store.Setup(s => s.LoadAllAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync((IReadOnlyList<GroupConfig>)
                 [new GroupConfig { Id = "y", GroupName = "Y" }, new GroupConfig { Id = "z", GroupName = "Z" }]);

        await sut.LoadGroupsAsync();

        sut.Groups.Select(g => g.Id).Should().BeEquivalentTo("y", "z");
    }

    [Fact]
    public async Task AddGroupAsync_PersistsAndAppendsAlphabetically()
    {
        var store = CreateStoreWith(
            new GroupConfig { Id = "a", GroupName = "Apps" },
            new GroupConfig { Id = "z", GroupName = "Zed" });
        var sut = CreateSut(store);
        await sut.LoadGroupsAsync();

        sut.NewGroupName = "Misc";
        await sut.AddGroupCommand.ExecuteAsync(null);

        store.Verify(s => s.SaveAsync(It.Is<GroupConfig>(c => c.GroupName == "Misc"), It.IsAny<CancellationToken>()), Times.Once);
        sut.Groups.Select(g => g.Name).Should().Equal("Apps", "Misc", "Zed");
        sut.NewGroupName.Should().BeEmpty("input is cleared after a successful add");
        sut.SelectedGroup.Should().NotBeNull();
        sut.SelectedGroup!.Name.Should().Be("Misc");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddGroupAsync_DoesNothing_ForBlankName(string? name)
    {
        var store = CreateStoreWith();
        var sut = CreateSut(store);
        sut.NewGroupName = name!;

        await sut.AddGroupCommand.ExecuteAsync(null);

        sut.Groups.Should().BeEmpty();
        store.Verify(s => s.SaveAsync(It.IsAny<GroupConfig>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddGroupAsync_TrimsWhitespaceAroundName()
    {
        var store = CreateStoreWith();
        var sut = CreateSut(store);
        sut.NewGroupName = "  Spaced  ";

        await sut.AddGroupCommand.ExecuteAsync(null);

        sut.Groups.Single().Name.Should().Be("Spaced");
    }

    [Fact]
    public async Task DeleteGroupAsync_RemovesAndCallsStore()
    {
        var configs = new[]
        {
            new GroupConfig { Id = "a", GroupName = "Apps" },
            new GroupConfig { Id = "b", GroupName = "Browsers" },
        };
        var store = CreateStoreWith(configs);
        var sut = CreateSut(store);
        await sut.LoadGroupsAsync();

        var toDelete = sut.Groups.Single(g => g.Id == "a");
        await sut.DeleteGroupCommand.ExecuteAsync(toDelete);

        store.Verify(s => s.DeleteAsync("a", It.IsAny<CancellationToken>()), Times.Once);
        sut.Groups.Select(g => g.Id).Should().ContainSingle().Which.Should().Be("b");
    }

    [Fact]
    public async Task DeleteGroupAsync_AdvancesSelection_ToNeighbourAtSameIndex()
    {
        var configs = new[]
        {
            new GroupConfig { Id = "a", GroupName = "A" },
            new GroupConfig { Id = "b", GroupName = "B" },
            new GroupConfig { Id = "c", GroupName = "C" },
        };
        var store = CreateStoreWith(configs);
        var sut = CreateSut(store);
        await sut.LoadGroupsAsync();
        sut.SelectedGroup = sut.Groups[1]; // B

        await sut.DeleteGroupCommand.ExecuteAsync(sut.Groups[1]);

        // B was at index 1 → selection moves to whatever is now at index 1 (the old C).
        sut.SelectedGroup.Should().NotBeNull();
        sut.SelectedGroup!.Id.Should().Be("c");
    }

    [Fact]
    public async Task DeleteGroupAsync_ClearsSelection_WhenLastGroupRemoved()
    {
        var store = CreateStoreWith(new GroupConfig { Id = "only", GroupName = "Only" });
        var sut = CreateSut(store);
        await sut.LoadGroupsAsync();
        sut.SelectedGroup = sut.Groups[0];

        await sut.DeleteGroupCommand.ExecuteAsync(sut.Groups[0]);

        sut.Groups.Should().BeEmpty();
        sut.SelectedGroup.Should().BeNull();
    }

    [Fact]
    public async Task DeleteGroupAsync_NoOps_ForNullArgument()
    {
        var store = CreateStoreWith(new GroupConfig { Id = "x", GroupName = "X" });
        var sut = CreateSut(store);
        await sut.LoadGroupsAsync();

        await sut.DeleteGroupCommand.ExecuteAsync(null);

        sut.Groups.Should().HaveCount(1);
        store.Verify(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
