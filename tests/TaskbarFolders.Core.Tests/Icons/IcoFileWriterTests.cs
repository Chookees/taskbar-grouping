using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FluentAssertions;
using TaskbarFolders.Core.Icons;
using Xunit;

namespace TaskbarFolders.Core.Tests.Icons;

public sealed class IcoFileWriterTests : IDisposable
{
    private readonly string _tempDir;

    public IcoFileWriterTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "TaskbarFolders.IcoTests." + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    private static RenderTargetBitmap SolidSquare(int size, Color color)
    {
        var visual = new DrawingVisual();
        using (var ctx = visual.RenderOpen())
        {
            ctx.DrawRectangle(new SolidColorBrush(color), null, new Rect(0, 0, size, size));
        }
        var bmp = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
        bmp.Render(visual);
        bmp.Freeze();
        return bmp;
    }

    [Fact]
    public void BuildIcoBytes_StartsWithCorrectIconDirHeader()
    {
        var source = SolidSquare(256, Colors.Magenta);

        var bytes = IcoFileWriter.BuildIcoBytes(source);

        bytes.Length.Should().BeGreaterThan(6 + 16 * 4);
        BitConverter.ToUInt16(bytes, 0).Should().Be(0, "reserved field must be 0");
        BitConverter.ToUInt16(bytes, 2).Should().Be(1, "type 1 = icon");
        BitConverter.ToUInt16(bytes, 4).Should().Be((ushort)IcoFileWriter.FrameSizes.Length);
    }

    [Fact]
    public void BuildIcoBytes_EmitsOneDirectoryEntryPerFrameSize()
    {
        var source = SolidSquare(256, Colors.Cyan);
        var bytes = IcoFileWriter.BuildIcoBytes(source);

        for (var i = 0; i < IcoFileWriter.FrameSizes.Length; i++)
        {
            var entry = 6 + i * 16;
            var expectedDim = IcoFileWriter.FrameSizes[i] >= 256 ? 0 : IcoFileWriter.FrameSizes[i];
            bytes[entry].Should().Be((byte)expectedDim, $"width for entry {i}");
            bytes[entry + 1].Should().Be((byte)expectedDim, $"height for entry {i}");
            bytes[entry + 4].Should().Be(1, "colour planes always 1");
            bytes[entry + 5].Should().Be(0);
            bytes[entry + 6].Should().Be(32, "bits-per-pixel always 32");
            bytes[entry + 7].Should().Be(0);

            var entryBytes = BitConverter.ToUInt32(bytes, entry + 8);
            var entryOffset = BitConverter.ToUInt32(bytes, entry + 12);
            entryBytes.Should().BeGreaterThan(0, "PNG payload should be non-empty");
            entryOffset.Should().BeGreaterThanOrEqualTo(6u + 16u * (uint)IcoFileWriter.FrameSizes.Length);
            (entryOffset + entryBytes).Should().BeLessThanOrEqualTo((uint)bytes.Length);
        }
    }

    [Fact]
    public async Task WriteAsync_WritesFileAtomically_AndLeavesNoTempBehind()
    {
        var sut = new IcoFileWriter();
        var target = Path.Combine(_tempDir, "out.ico");

        await sut.WriteAsync(SolidSquare(256, Colors.Orange), target);

        File.Exists(target).Should().BeTrue();
        Directory.EnumerateFiles(_tempDir, "*.tmp").Should().BeEmpty();
    }

    [Fact]
    public async Task WriteAsync_CreatesMissingDirectory()
    {
        var sut = new IcoFileWriter();
        var nestedTarget = Path.Combine(_tempDir, "newsub", "deeper", "icon.ico");

        await sut.WriteAsync(SolidSquare(256, Colors.Red), nestedTarget);

        File.Exists(nestedTarget).Should().BeTrue();
    }

    [Fact]
    public async Task WriteAsync_ProducesFile_DecodableByIconBitmapDecoder()
    {
        var sut = new IcoFileWriter();
        var target = Path.Combine(_tempDir, "decode.ico");
        await sut.WriteAsync(SolidSquare(256, Colors.Lime), target);

        var decoder = new IconBitmapDecoder(
            new Uri(target, UriKind.Absolute),
            BitmapCreateOptions.PreservePixelFormat,
            BitmapCacheOption.OnLoad);

        decoder.Frames.Count.Should().Be(IcoFileWriter.FrameSizes.Length);
        decoder.Frames.Select(f => f.PixelWidth).Should().BeEquivalentTo(IcoFileWriter.FrameSizes);
    }

    [Fact]
    public async Task WriteAsync_OverwritesExistingFile()
    {
        var sut = new IcoFileWriter();
        var target = Path.Combine(_tempDir, "twice.ico");

        await sut.WriteAsync(SolidSquare(256, Colors.Red), target);
        var firstSize = new FileInfo(target).Length;
        firstSize.Should().BeGreaterThan(0);

        await sut.WriteAsync(SolidSquare(256, Colors.Blue), target);
        File.Exists(target).Should().BeTrue();
    }

    [Fact]
    public void BuildIcoBytes_ThrowsForNullSource()
    {
        var act = () => IcoFileWriter.BuildIcoBytes(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task WriteAsync_ThrowsForBlankPath(string? path)
    {
        var sut = new IcoFileWriter();
        Func<Task> act = () => sut.WriteAsync(SolidSquare(256, Colors.Red), path!);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
