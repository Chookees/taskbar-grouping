using FluentAssertions;
using TaskbarFolders.Launcher.Services;
using Xunit;

namespace TaskbarFolders.Launcher.Tests.Services;

public class ProcessLauncherTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Launch_ReturnsFalse_ForBlankPath(string? path)
    {
        var sut = new ProcessLauncher();

        var ok = sut.Launch(path!, arguments: null);

        ok.Should().BeFalse();
    }

    [Fact]
    public void Launch_ReturnsFalse_ForNonexistentExecutable()
    {
        var sut = new ProcessLauncher();

        var ok = sut.Launch("C:/this/does/not/exist-xyz.exe", arguments: null);

        ok.Should().BeFalse("Process.Start with UseShellExecute throws Win32Exception for missing files; ProcessLauncher catches it");
    }
}
