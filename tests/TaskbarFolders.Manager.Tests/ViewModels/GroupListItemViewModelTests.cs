using FluentAssertions;
using TaskbarFolders.Manager.ViewModels;
using TaskbarFolders.Shared.Models;
using Xunit;

namespace TaskbarFolders.Manager.Tests.ViewModels;

public class GroupListItemViewModelTests
{
    [Fact]
    public void Constructor_ProjectsConfigFields()
    {
        var config = new GroupConfig
        {
            Id = "id-1",
            GroupName = "Tools",
            Apps =
            {
                new AppEntry { Name = "a", Path = "a.exe" },
                new AppEntry { Name = "b", Path = "b.exe" },
            },
        };

        var sut = new GroupListItemViewModel(config);

        sut.Id.Should().Be("id-1");
        sut.Name.Should().Be("Tools");
        sut.AppCount.Should().Be(2);
    }

    [Fact]
    public void Name_AssignmentPropagatesToWrappedConfig()
    {
        var config = new GroupConfig { Id = "x", GroupName = "Old" };
        var sut = new GroupListItemViewModel(config);

        sut.Name = "New";

        config.GroupName.Should().Be("New", "the wrapper writes through to its config so saves see the new name");
    }
}
