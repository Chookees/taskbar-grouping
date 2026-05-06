using System.Windows.Media;
using System.Windows.Media.Imaging;
using FluentAssertions;
using TaskbarFolders.Core.Icons;
using Xunit;

namespace TaskbarFolders.Core.Tests.Icons;

public class CompositeIconGeneratorTests
{
    private readonly CompositeIconGenerator _sut = new();

    [Fact]
    public void GenerateComposite_WithSingleIcon_ReturnsCorrectSize()
    {
        var icons = new List<BitmapSource> { CreateTestIcon(64) };

        BitmapSource result = _sut.GenerateComposite(icons, 256);

        result.PixelWidth.Should().Be(256);
        result.PixelHeight.Should().Be(256);
    }

    [Fact]
    public void GenerateComposite_WithFourIcons_ReturnsCorrectSize()
    {
        var icons = new List<BitmapSource>
        {
            CreateTestIcon(64),
            CreateTestIcon(64),
            CreateTestIcon(64),
            CreateTestIcon(64),
        };

        BitmapSource result = _sut.GenerateComposite(icons, 256);

        result.PixelWidth.Should().Be(256);
        result.PixelHeight.Should().Be(256);
    }

    [Fact]
    public void GenerateComposite_WithCustomSize_ReturnsRequestedSize()
    {
        var icons = new List<BitmapSource> { CreateTestIcon(32) };

        BitmapSource result = _sut.GenerateComposite(icons, 128);

        result.PixelWidth.Should().Be(128);
        result.PixelHeight.Should().Be(128);
    }

    [Fact]
    public void GenerateComposite_WithNullIcons_ThrowsArgumentNullException()
    {
        var act = () => _sut.GenerateComposite(null!, 256);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GenerateComposite_WithZeroSize_ThrowsArgumentOutOfRangeException()
    {
        var icons = new List<BitmapSource> { CreateTestIcon(32) };

        var act = () => _sut.GenerateComposite(icons, 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GenerateComposite_ResultIsFrozen()
    {
        var icons = new List<BitmapSource> { CreateTestIcon(32) };

        BitmapSource result = _sut.GenerateComposite(icons, 256);

        result.IsFrozen.Should().BeTrue();
    }

    private static BitmapSource CreateTestIcon(int size)
    {
        var bitmap = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
        var visual = new DrawingVisual();
        using (DrawingContext ctx = visual.RenderOpen())
        {
            ctx.DrawRectangle(Brushes.Blue, null, new System.Windows.Rect(0, 0, size, size));
        }
        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
    }
}
