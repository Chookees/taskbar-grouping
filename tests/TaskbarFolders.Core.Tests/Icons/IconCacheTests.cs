using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FluentAssertions;
using TaskbarFolders.Core.Icons;
using Xunit;

namespace TaskbarFolders.Core.Tests.Icons;

public class IconCacheTests : IDisposable
{
    private readonly string _tempCacheDir;
    private readonly IconCache _sut;

    public IconCacheTests()
    {
        _tempCacheDir = Path.Combine(Path.GetTempPath(), $"iconcache_test_{Guid.NewGuid():N}");
        _sut = new IconCache(_tempCacheDir);
    }

    [Fact]
    public void GetOrCreate_FirstCall_InvokesFactory()
    {
        bool factoryCalled = false;

        _sut.GetOrCreate("test-key", () =>
        {
            factoryCalled = true;
            return CreateTestBitmap();
        });

        factoryCalled.Should().BeTrue();
    }

    [Fact]
    public void GetOrCreate_SecondCall_ReturnsCached()
    {
        int callCount = 0;
        BitmapSource Factory()
        {
            callCount++;
            return CreateTestBitmap();
        }

        _sut.GetOrCreate("same-key", Factory);
        _sut.GetOrCreate("same-key", Factory);

        callCount.Should().Be(1);
    }

    [Fact]
    public void GetOrCreate_FactoryReturnsNull_ReturnsNull()
    {
        BitmapSource? result = _sut.GetOrCreate("null-key", () => null);

        result.Should().BeNull();
    }

    [Fact]
    public void Invalidate_RemovesFromCache()
    {
        int callCount = 0;
        _sut.GetOrCreate("inv-key", () => { callCount++; return CreateTestBitmap(); });
        _sut.Invalidate("inv-key");
        _sut.GetOrCreate("inv-key", () => { callCount++; return CreateTestBitmap(); });

        callCount.Should().Be(2);
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        int callCount = 0;
        _sut.GetOrCreate("key1", () => { callCount++; return CreateTestBitmap(); });
        _sut.GetOrCreate("key2", () => { callCount++; return CreateTestBitmap(); });

        _sut.Clear();

        _sut.GetOrCreate("key1", () => { callCount++; return CreateTestBitmap(); });
        _sut.GetOrCreate("key2", () => { callCount++; return CreateTestBitmap(); });

        callCount.Should().Be(4);
    }

    [Fact]
    public void GetOrCreate_WithNullKey_ThrowsArgumentException()
    {
        var act = () => _sut.GetOrCreate(null!, () => CreateTestBitmap());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetOrCreate_WithNullFactory_ThrowsArgumentNullException()
    {
        var act = () => _sut.GetOrCreate("key", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempCacheDir))
            Directory.Delete(_tempCacheDir, true);
    }

    private static RenderTargetBitmap CreateTestBitmap()
    {
        var bitmap = new RenderTargetBitmap(64, 64, 96, 96, PixelFormats.Pbgra32);
        var visual = new DrawingVisual();
        using (DrawingContext ctx = visual.RenderOpen())
        {
            ctx.DrawRectangle(Brushes.Green, null, new System.Windows.Rect(0, 0, 64, 64));
        }
        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
    }
}
