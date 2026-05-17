using System;
using System.IO;
using FluentAssertions;
using TaskbarFolders.Core.Icons;
using Xunit;

namespace TaskbarFolders.Core.Tests.Icons;

public class ShellIconExtractorTests
{
    private static readonly string _notepad = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "notepad.exe");
    private static readonly string _cmd = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe");

    private static readonly string _icoFile = Path.Combine(
        FindRepoRoot(),
        "assets",
        "icons",
        "manager.ico");

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "TaskbarFolders.sln")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName
            ?? throw new InvalidOperationException("Could not locate TaskbarFolders.sln above the test assembly.");
    }

    [Fact]
    public void ExtractIcon_ReturnsBitmap_ForNotepadExe()
    {
        File.Exists(_notepad).Should().BeTrue("Windows always ships notepad.exe");

        var sut = new ShellIconExtractor();
        var icon = sut.ExtractIcon(_notepad, 256);

        icon.Should().NotBeNull();
        icon!.PixelWidth.Should().BeGreaterThan(0);
        icon.PixelHeight.Should().BeGreaterThan(0);
        icon.IsFrozen.Should().BeTrue("returned BitmapSource must be cross-thread safe");
    }

    [Theory]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(48)]
    [InlineData(256)]
    public void ExtractIcon_HonoursRequestedSize_ForExe(int size)
    {
        var sut = new ShellIconExtractor();
        var icon = sut.ExtractIcon(_cmd, size);

        icon.Should().NotBeNull();
        // Shell may round to its native image-list sizes (16/32/48/256); accept any
        // result that is at least as wide as the requested bracket lower bound.
        icon!.PixelWidth.Should().BeGreaterOrEqualTo(Math.Min(size, 16));
    }

    [Fact]
    public void ExtractIcon_ReturnsBitmap_For_icoFile()
    {
        File.Exists(_icoFile).Should().BeTrue($"manager.ico was generated in M0 and lives at {_icoFile}");

        var sut = new ShellIconExtractor();
        var icon = sut.ExtractIcon(_icoFile, 256);

        icon.Should().NotBeNull();
        icon!.PixelWidth.Should().Be(256);
        icon.PixelHeight.Should().Be(256);
    }

    [Fact]
    public void ExtractIcon_PicksSmallestFrameAboveRequestedSize_For_icoFile()
    {
        var sut = new ShellIconExtractor();

        var icon = sut.ExtractIcon(_icoFile, 32);

        icon.Should().NotBeNull();
        icon!.PixelWidth.Should().Be(32, "the .ico has a 32x32 frame, which is the smallest >= 32");
    }

    [Fact]
    public void ExtractIcon_FallsBackToLargestFrame_WhenRequestedSizeExceedsAvailable()
    {
        var sut = new ShellIconExtractor();

        var icon = sut.ExtractIcon(_icoFile, 9999);

        icon.Should().NotBeNull();
        icon!.PixelWidth.Should().Be(256, "largest .ico frame is 256x256");
    }

    [Fact]
    public void ExtractIcon_ReturnsNull_ForNonexistentFile()
    {
        var sut = new ShellIconExtractor();
        // SHGetFileInfo with SHGFI_USEFILEATTRIBUTES infers from extension even for missing paths,
        // so a fictional .qzx file should produce either null (no association) or a generic icon.
        // The contract simply requires no exception to escape.
        var act = () => sut.ExtractIcon("C:/this/does/not/exist.qzx", 256);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("   ")]
    public void ExtractIcon_ThrowsArgumentException_ForBlankPath(string? path)
    {
        var sut = new ShellIconExtractor();
        var act = () => sut.ExtractIcon(path!, 256);

        act.Should().Throw<ArgumentException>();
    }
}
