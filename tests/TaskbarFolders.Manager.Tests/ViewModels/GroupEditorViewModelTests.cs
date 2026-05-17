using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FluentAssertions;
using Moq;
using TaskbarFolders.Core.Icons;
using TaskbarFolders.Manager.ViewModels;
using TaskbarFolders.Shared.Configuration;
using TaskbarFolders.Shared.Models;
using Xunit;

namespace TaskbarFolders.Manager.Tests.ViewModels;

public class GroupEditorViewModelTests
{
    private static BitmapSource StubIcon()
    {
        var bmp = new RenderTargetBitmap(32, 32, 96, 96, PixelFormats.Pbgra32);
        var visual = new DrawingVisual();
        using (var ctx = visual.RenderOpen())
        {
            ctx.DrawRectangle(Brushes.Red, null, new Rect(0, 0, 32, 32));
        }
        bmp.Render(visual);
        bmp.Freeze();
        return bmp;
    }

    private static (GroupEditorViewModel sut,
                    Mock<IIconExtractor> extractor,
                    Mock<ICompositeIconGenerator> composer,
                    Mock<IIconCache> cache,
                    Mock<IGroupConfigStore> store) CreateSut()
    {
        var extractor = new Mock<IIconExtractor>();
        extractor.Setup(e => e.ExtractIcon(It.IsAny<string>(), It.IsAny<int>())).Returns(StubIcon());

        var composer = new Mock<ICompositeIconGenerator>();
        composer.Setup(c => c.GenerateComposite(It.IsAny<System.Collections.Generic.IReadOnlyList<BitmapSource>>(), It.IsAny<int>()))
                .Returns(StubIcon());

        var cache = new Mock<IIconCache>();
        BitmapSource? unused;
        cache.Setup(c => c.TryGet(It.IsAny<string>(), It.IsAny<int>(), out unused)).Returns(false);

        var store = new Mock<IGroupConfigStore>();
        store.Setup(s => s.SaveAsync(It.IsAny<GroupConfig>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var sut = new GroupEditorViewModel(extractor.Object, composer.Object, cache.Object, store.Object);
        return (sut, extractor, composer, cache, store);
    }

    [Fact]
    public void Bind_Null_ClearsAppsAndPreview()
    {
        var (sut, _, _, _, _) = CreateSut();
        var config = new GroupConfig { Id = "g", GroupName = "g", Apps = { new AppEntry { Name = "a", Path = "a.exe" } } };
        sut.Bind(new GroupListItemViewModel(config));
        sut.Apps.Should().NotBeEmpty();

        sut.Bind(null);

        sut.Apps.Should().BeEmpty();
        sut.BoundItem.Should().BeNull();
        sut.CompositeIconPreview.Should().BeNull();
    }

    [Fact]
    public void Bind_PopulatesAppsFromConfig_AndExtractsIcons()
    {
        var (sut, extractor, _, _, _) = CreateSut();
        var config = new GroupConfig
        {
            Id = "g",
            GroupName = "g",
            Apps =
            {
                new AppEntry { Name = "A", Path = "a.exe" },
                new AppEntry { Name = "B", Path = "b.exe" },
            },
        };

        sut.Bind(new GroupListItemViewModel(config));

        sut.Apps.Should().HaveCount(2);
        sut.Apps.Select(a => a.Name).Should().Equal("A", "B");
        extractor.Verify(e => e.ExtractIcon("a.exe", GroupEditorViewModel.PreviewSize), Times.Once);
        extractor.Verify(e => e.ExtractIcon("b.exe", GroupEditorViewModel.PreviewSize), Times.Once);
    }

    [Fact]
    public async Task AddAppsAsync_AppendsValidExeAndLnk_AndPersists()
    {
        var (sut, _, _, _, store) = CreateSut();
        var config = new GroupConfig { Id = "g", GroupName = "g" };
        var item = new GroupListItemViewModel(config);
        sut.Bind(item);

        await sut.AddAppsCommand.ExecuteAsync(new[] { "C:/x/notepad.exe", "C:/y/link.lnk" });

        sut.Apps.Should().HaveCount(2);
        config.Apps.Should().HaveCount(2, "the wrapper writes through to the underlying config");
        sut.Apps[0].Name.Should().Be("notepad");
        sut.Apps[1].Name.Should().Be("link");
        store.Verify(s => s.SaveAsync(config, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("C:/some/document.pdf")]
    [InlineData("C:/some/image.png")]
    [InlineData("C:/some/binary.dll")]
    public async Task AddAppsAsync_SkipsNonExecutableExtensions(string nonExePath)
    {
        var (sut, _, _, _, store) = CreateSut();
        sut.Bind(new GroupListItemViewModel(new GroupConfig { Id = "g", GroupName = "g" }));

        await sut.AddAppsCommand.ExecuteAsync(new[] { nonExePath });

        sut.Apps.Should().BeEmpty();
        store.Verify(s => s.SaveAsync(It.IsAny<GroupConfig>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddAppsAsync_NoOp_WhenNothingBound()
    {
        var (sut, _, _, _, store) = CreateSut();

        await sut.AddAppsCommand.ExecuteAsync(new[] { "x.exe" });

        sut.Apps.Should().BeEmpty();
        store.Verify(s => s.SaveAsync(It.IsAny<GroupConfig>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveAppAsync_RemovesFromBothViewModelAndConfig_AndPersists()
    {
        var (sut, _, _, _, store) = CreateSut();
        var config = new GroupConfig
        {
            Id = "g",
            GroupName = "g",
            Apps = { new AppEntry { Name = "a", Path = "a.exe" } },
        };
        sut.Bind(new GroupListItemViewModel(config));
        var target = sut.Apps.Single();

        await sut.RemoveAppCommand.ExecuteAsync(target);

        sut.Apps.Should().BeEmpty();
        config.Apps.Should().BeEmpty();
        store.Verify(s => s.SaveAsync(config, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PreviewRefresh_DebouncesAndCallsComposer()
    {
        var (sut, _, composer, _, _) = CreateSut();
        var config = new GroupConfig { Id = "g", GroupName = "g" };
        sut.Bind(new GroupListItemViewModel(config));

        await sut.AddAppsCommand.ExecuteAsync(new[] { "a.exe", "b.exe", "c.exe" });

        // Wait past the debounce window so the scheduled refresh fires.
        await Task.Delay(GroupEditorViewModel.PreviewDebounce + TimeSpan.FromMilliseconds(150));

        composer.Verify(
            c => c.GenerateComposite(It.IsAny<System.Collections.Generic.IReadOnlyList<BitmapSource>>(), GroupEditorViewModel.PreviewSize),
            Times.AtLeastOnce);
        sut.CompositeIconPreview.Should().NotBeNull();
    }

    [Fact]
    public void Dispose_StopsRespondingToFurtherCollectionChanges()
    {
        var (sut, _, composer, _, _) = CreateSut();
        sut.Bind(new GroupListItemViewModel(new GroupConfig { Id = "g", GroupName = "g" }));

        sut.Dispose();
        sut.Apps.Add(new AppEntryViewModel(new AppEntry { Name = "x", Path = "x.exe" }));

        // No exception, and the composer must not be invoked for the post-dispose mutation.
        composer.Verify(
            c => c.GenerateComposite(It.IsAny<System.Collections.Generic.IReadOnlyList<BitmapSource>>(), It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task RemoveAppAsync_NoOp_ForNullArgument()
    {
        var (sut, _, _, _, store) = CreateSut();
        sut.Bind(new GroupListItemViewModel(new GroupConfig { Id = "g", GroupName = "g" }));

        await sut.RemoveAppCommand.ExecuteAsync(null);

        store.Verify(s => s.SaveAsync(It.IsAny<GroupConfig>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
