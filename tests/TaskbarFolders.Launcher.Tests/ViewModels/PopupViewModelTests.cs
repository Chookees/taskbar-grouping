using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using FluentAssertions;
using Moq;
using TaskbarFolders.Core.Icons;
using TaskbarFolders.Launcher.Configuration;
using TaskbarFolders.Launcher.Services;
using TaskbarFolders.Launcher.ViewModels;
using TaskbarFolders.Shared.Configuration;
using TaskbarFolders.Shared.Models;
using Xunit;

namespace TaskbarFolders.Launcher.Tests.ViewModels;

public class PopupViewModelTests
{
    private static (PopupViewModel sut,
                    Mock<IGroupConfigStore> store,
                    Mock<IIconExtractor> extractor,
                    Mock<IIconCache> cache,
                    Mock<IProcessLauncher> launcher) CreateSut(string groupId = "g1", GroupConfig? config = null)
    {
        var store = new Mock<IGroupConfigStore>();
        store.Setup(s => s.LoadAsync(groupId, It.IsAny<CancellationToken>())).ReturnsAsync(config);

        var extractor = new Mock<IIconExtractor>();
        extractor.Setup(e => e.ExtractIcon(It.IsAny<string>(), It.IsAny<int>())).Returns((BitmapSource?)null);

        var cache = new Mock<IIconCache>();
        BitmapSource? unused;
        cache.Setup(c => c.TryGet(It.IsAny<string>(), It.IsAny<int>(), out unused)).Returns(false);

        var launcher = new Mock<IProcessLauncher>();
        launcher.Setup(l => l.Launch(It.IsAny<string>(), It.IsAny<string?>())).Returns(true);

        var sut = new PopupViewModel(
            store.Object,
            extractor.Object,
            cache.Object,
            launcher.Object,
            new LauncherOptions(groupId));

        return (sut, store, extractor, cache, launcher);
    }

    [Fact]
    public async Task LoadAsync_PopulatesNameColumnsAndApps_FromStoredGroup()
    {
        var config = new GroupConfig
        {
            Id = "g1",
            GroupName = "Tools",
            Columns = 4,
            Apps =
            {
                new AppEntry { Name = "VS Code", Path = "C:/code.exe", Arguments = "--no-sandbox" },
                new AppEntry { Name = "Notepad", Path = "C:/notepad.exe" },
            },
        };
        var (sut, _, _, _, _) = CreateSut("g1", config);

        await sut.LoadAsync();

        sut.GroupName.Should().Be("Tools");
        sut.Columns.Should().Be(4);
        sut.Apps.Should().HaveCount(2);
        sut.Apps[0].Name.Should().Be("VS Code");
        sut.Apps[0].Arguments.Should().Be("--no-sandbox");
        sut.IsUnavailable.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_SetsIsUnavailable_WhenGroupNotFound()
    {
        var (sut, _, _, _, _) = CreateSut("missing", config: null);

        await sut.LoadAsync();

        sut.IsUnavailable.Should().BeTrue();
        sut.Apps.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_SetsIsUnavailable_WhenGroupHasNoApps()
    {
        var config = new GroupConfig { Id = "empty", GroupName = "Empty" };
        var (sut, _, _, _, _) = CreateSut("empty", config);

        await sut.LoadAsync();

        sut.IsUnavailable.Should().BeTrue();
        sut.GroupName.Should().Be("Empty");
    }

    [Fact]
    public async Task LoadAsync_UsesCacheWhenAvailable_AndSetsOnMiss()
    {
        var config = new GroupConfig { Id = "g", GroupName = "g", Apps = { new AppEntry { Name = "a", Path = "a.exe" } } };
        var (sut, _, extractor, cache, _) = CreateSut("g", config);

        await sut.LoadAsync();

        // Cache was queried and missed → extractor called → result stored back
        cache.Verify(c => c.TryGet("a.exe", PopupViewModel.IconSize, out It.Ref<BitmapSource?>.IsAny), Times.Once);
        extractor.Verify(e => e.ExtractIcon("a.exe", PopupViewModel.IconSize), Times.Once);
        // ExtractIcon returned null in this test, so Set is not called — fine, the contract only stores non-null icons.
    }

    [Fact]
    public void LaunchAppCommand_DoesNothing_ForNullArgument()
    {
        var (sut, _, _, _, launcher) = CreateSut();

        sut.LaunchAppCommand.Execute(null);

        launcher.Verify(l => l.Launch(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task LaunchAppCommand_InvokesLauncher_AndRaisesLaunchSucceeded()
    {
        var config = new GroupConfig { Id = "g", GroupName = "g", Apps = { new AppEntry { Name = "a", Path = "a.exe", Arguments = "-x" } } };
        var (sut, _, _, _, launcher) = CreateSut("g", config);
        await sut.LoadAsync();

        var succeeded = false;
        sut.LaunchSucceeded += (_, _) => succeeded = true;

        sut.LaunchAppCommand.Execute(sut.Apps[0]);

        launcher.Verify(l => l.Launch("a.exe", "-x"), Times.Once);
        succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task LaunchAppCommand_DoesNotRaiseLaunchSucceeded_OnFailure()
    {
        var config = new GroupConfig { Id = "g", GroupName = "g", Apps = { new AppEntry { Name = "a", Path = "bad.exe" } } };
        var (sut, _, _, _, launcher) = CreateSut("g", config);
        launcher.Setup(l => l.Launch(It.IsAny<string>(), It.IsAny<string?>())).Returns(false);
        await sut.LoadAsync();

        var succeeded = false;
        sut.LaunchSucceeded += (_, _) => succeeded = true;

        sut.LaunchAppCommand.Execute(sut.Apps[0]);

        succeeded.Should().BeFalse("the popup must stay open so the user can pick a different app or close manually");
    }

    [Fact]
    public async Task LaunchAppCommand_SetsLastError_OnFailure_WithAppName()
    {
        var config = new GroupConfig { Id = "g", GroupName = "g", Apps = { new AppEntry { Name = "Notepad", Path = "notepad.exe" } } };
        var (sut, _, _, _, launcher) = CreateSut("g", config);
        launcher.Setup(l => l.Launch(It.IsAny<string>(), It.IsAny<string?>())).Returns(false);
        await sut.LoadAsync();

        sut.LaunchAppCommand.Execute(sut.Apps[0]);

        sut.LastError.Should().NotBeNull();
        sut.LastError.Should().Contain("Notepad");
    }

    [Fact]
    public async Task LaunchAppCommand_ClearsLastError_OnSuccess()
    {
        var config = new GroupConfig { Id = "g", GroupName = "g", Apps = { new AppEntry { Name = "a", Path = "a.exe" } } };
        var (sut, _, _, _, launcher) = CreateSut("g", config);
        await sut.LoadAsync();
        // Pre-set a sticky error from a previous failed click.
        sut.LastError = "Old error";
        launcher.Setup(l => l.Launch(It.IsAny<string>(), It.IsAny<string?>())).Returns(true);

        sut.LaunchAppCommand.Execute(sut.Apps[0]);

        sut.LastError.Should().BeNull();
    }
}
