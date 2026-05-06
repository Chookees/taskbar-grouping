using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FluentAssertions;
using TaskbarFolders.Core.Icons;
using Xunit;

namespace TaskbarFolders.Core.Tests.Icons;

public class IcoWriterTests
{
    [Fact]
    public void Write_ToStream_ProducesValidIcoHeader()
    {
        BitmapSource source = CreateTestBitmap(256);
        using var stream = new MemoryStream();

        IcoWriter.Write(source, stream);

        stream.Position = 0;
        using var reader = new BinaryReader(stream);

        ushort reserved = reader.ReadUInt16();
        ushort type = reader.ReadUInt16();
        ushort count = reader.ReadUInt16();

        reserved.Should().Be(0);
        type.Should().Be(1);
        count.Should().Be(4);
    }

    [Fact]
    public void Write_ToStream_ProducesNonEmptyOutput()
    {
        BitmapSource source = CreateTestBitmap(256);
        using var stream = new MemoryStream();

        IcoWriter.Write(source, stream);

        stream.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Write_ToFile_CreatesFile()
    {
        BitmapSource source = CreateTestBitmap(256);
        string tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.ico");

        try
        {
            IcoWriter.Write(source, tempPath);

            File.Exists(tempPath).Should().BeTrue();
            new FileInfo(tempPath).Length.Should().BeGreaterThan(0);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    [Fact]
    public void Write_WithNullSource_ThrowsArgumentNullException()
    {
        using var stream = new MemoryStream();

        var act = () => IcoWriter.Write(null!, stream);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Write_WithNullStream_ThrowsArgumentNullException()
    {
        BitmapSource source = CreateTestBitmap(64);

        var act = () => IcoWriter.Write(source, (Stream)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Write_CreatesDirectoryIfNotExists()
    {
        BitmapSource source = CreateTestBitmap(256);
        string tempDir = Path.Combine(Path.GetTempPath(), $"icotest_{Guid.NewGuid():N}");
        string tempPath = Path.Combine(tempDir, "test.ico");

        try
        {
            IcoWriter.Write(source, tempPath);

            Directory.Exists(tempDir).Should().BeTrue();
            File.Exists(tempPath).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    private static BitmapSource CreateTestBitmap(int size)
    {
        var bitmap = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
        var visual = new DrawingVisual();
        using (DrawingContext ctx = visual.RenderOpen())
        {
            ctx.DrawRectangle(Brushes.Red, null, new System.Windows.Rect(0, 0, size, size));
        }
        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
    }
}
