using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using FluentAssertions;
using Moq;
using TaskbarFolders.Core.Icons;
using TaskbarFolders.Core.Shortcuts;
using TaskbarFolders.Manager.Services;
using TaskbarFolders.Shared.Configuration;
using TaskbarFolders.Shared.Models;
using Xunit;

namespace TaskbarFolders.Manager.Tests.Services;

public sealed class GroupSyncServiceTests : IDisposable
{
    private readonly string _tempBase;
    private readonly AppDataPathProvider _paths;
    private readonly Mock<IIconExtractor> _extractor;
    private readonly Mock<ICompositeIconGenerator> _composer;
    private readonly Mock<IIcoFileWriter> _icoWriter;
    private readonly Mock<IIconCache> _cache;
    private readonly Mock<IShortcutGenerator> _shortcutGenerator;
    private readonly Mock<ILauncherPathResolver> _launcherResolver;

    public GroupSyncServiceTests()
    {
        _tempBase = Path.Combine(Path.GetTempPath(), "TaskbarFolders.Sync." + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempBase);
        _paths = new AppDataPathProvider(_tempBase);

        _extractor = new Mock<IIconExtractor>();
        _extractor.Setup(e => e.ExtractIcon(It.IsAny<string>(), It.IsAny<int>())).Returns((BitmapSource?)null);

        _composer = new Mock<ICompositeIconGenerator>();
        _composer.Setup(c => c.GenerateComposite(It.IsAny<System.Collections.Generic.IReadOnlyList<BitmapSource>>(), It.IsAny<int>()))
                 .Returns(StubIcon());

        _icoWriter = new Mock<IIcoFileWriter>();
        _icoWriter.Setup(w => w.WriteAsync(It.IsAny<BitmapSource>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

        _cache = new Mock<IIconCache>();
        BitmapSource? unused;
        _cache.Setup(c => c.TryGet(It.IsAny<string>(), It.IsAny<int>(), out unused)).Returns(false);

        _shortcutGenerator = new Mock<IShortcutGenerator>();
        _launcherResolver = new Mock<ILauncherPathResolver>();
        _launcherResolver.Setup(r => r.TryResolve()).Returns("C:/install/Launcher.exe");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempBase))
        {
            Directory.Delete(_tempBase, recursive: true);
        }
        GC.SuppressFinalize(this);
    }

    private static BitmapSource StubIcon()
    {
        var visual = new System.Windows.Media.DrawingVisual();
        using (var ctx = visual.RenderOpen())
        {
            ctx.DrawRectangle(System.Windows.Media.Brushes.Magenta, null, new System.Windows.Rect(0, 0, 32, 32));
        }
        var bmp = new RenderTargetBitmap(32, 32, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
        bmp.Render(visual);
        bmp.Freeze();
        return bmp;
    }

    private GroupSyncService CreateSut() => new(
        _paths,
        _extractor.Object,
        _composer.Object,
        _icoWriter.Object,
        _cache.Object,
        _shortcutGenerator.Object,
        _launcherResolver.Object);

    [Fact]
    public async Task SyncAsync_NoOp_ForEmptyGroup()
    {
        var sut = CreateSut();
        var config = new GroupConfig { Id = "g", GroupName = "g" };

        await sut.SyncAsync(config);

        _icoWriter.VerifyNoOtherCalls();
        _shortcutGenerator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SyncAsync_NoOp_WhenLauncherCannotBeResolved()
    {
        _launcherResolver.Setup(r => r.TryResolve()).Returns((string?)null);
        var sut = CreateSut();
        var config = new GroupConfig
        {
            Id = "g",
            GroupName = "g",
            Apps = { new AppEntry { Name = "a", Path = "a.exe" } },
        };

        await sut.SyncAsync(config);

        _shortcutGenerator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SyncAsync_NoOp_WhenNoIconsCouldBeExtracted()
    {
        _extractor.Setup(e => e.ExtractIcon(It.IsAny<string>(), It.IsAny<int>())).Returns((BitmapSource?)null);
        var sut = CreateSut();
        var config = new GroupConfig
        {
            Id = "g",
            GroupName = "g",
            Apps = { new AppEntry { Name = "a", Path = "missing.exe" } },
        };

        await sut.SyncAsync(config);

        // Composer must not be called with an empty icon list
        _composer.VerifyNoOtherCalls();
        _shortcutGenerator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SyncAsync_WritesIcoAndShortcut_WhenIconsExtractSuccessfully()
    {
        _extractor.Setup(e => e.ExtractIcon(It.IsAny<string>(), It.IsAny<int>())).Returns(StubIcon());
        var sut = CreateSut();
        var config = new GroupConfig
        {
            Id = "g",
            GroupName = "Tools",
            Apps =
            {
                new AppEntry { Name = "a", Path = "a.exe" },
                new AppEntry { Name = "b", Path = "b.exe" },
            },
        };

        await sut.SyncAsync(config);

        _icoWriter.Verify(w => w.WriteAsync(
            It.IsAny<BitmapSource>(),
            _paths.GetGroupIconFile("g"),
            It.IsAny<CancellationToken>()), Times.Once);

        _shortcutGenerator.Verify(g => g.Generate(It.Is<GroupShortcutRequest>(r =>
            r.GroupId == "g" &&
            r.DisplayName == "Tools" &&
            r.TargetExePath == "C:/install/Launcher.exe" &&
            r.IconPath == _paths.GetGroupIconFile("g") &&
            r.ShortcutPath == _paths.GetGroupShortcutFile("g"))), Times.Once);
    }

    [Fact]
    public async Task SyncAsync_CapsCompositeAtMaxTiles()
    {
        _extractor.Setup(e => e.ExtractIcon(It.IsAny<string>(), It.IsAny<int>())).Returns(StubIcon());
        var sut = CreateSut();
        var config = new GroupConfig
        {
            Id = "g",
            GroupName = "g",
            Apps =
            {
                new AppEntry { Name = "1", Path = "1.exe" },
                new AppEntry { Name = "2", Path = "2.exe" },
                new AppEntry { Name = "3", Path = "3.exe" },
                new AppEntry { Name = "4", Path = "4.exe" },
                new AppEntry { Name = "5", Path = "5.exe" },
                new AppEntry { Name = "6", Path = "6.exe" },
            },
        };

        await sut.SyncAsync(config);

        // Only the first four are passed to the extractor (cap = CompositeIconGenerator.MaxTiles = 4)
        _extractor.Verify(e => e.ExtractIcon(It.IsAny<string>(), GroupSyncService.CompositeSourceIconSize), Times.Exactly(4));
    }

    [Fact]
    public void RemoveArtifacts_DeletesIconAndShortcut_IfPresent()
    {
        Directory.CreateDirectory(_paths.IconsDirectory);
        Directory.CreateDirectory(_paths.ShortcutsDirectory);
        var iconPath = _paths.GetGroupIconFile("g");
        var lnkPath = _paths.GetGroupShortcutFile("g");
        File.WriteAllBytes(iconPath, [0]);
        File.WriteAllBytes(lnkPath, [0]);

        var sut = CreateSut();
        sut.RemoveArtifacts("g");

        File.Exists(iconPath).Should().BeFalse();
        File.Exists(lnkPath).Should().BeFalse();
    }

    [Fact]
    public void RemoveArtifacts_IsNoOp_WhenFilesMissing()
    {
        var sut = CreateSut();

        var act = () => sut.RemoveArtifacts("never-existed");

        act.Should().NotThrow();
    }
}
