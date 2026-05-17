using FluentAssertions;
using TaskbarFolders.Manager.Services;
using Xunit;

namespace TaskbarFolders.Manager.Tests.Services;

public class LauncherPathResolverTests
{
    [Fact]
    public void TryResolve_DoesNotThrow_AndReturnsNullOrAbsolutePath()
    {
        // Test environment is the test bin directory, so the side-by-side check will likely
        // miss (TaskbarFolders.Launcher.exe lives in its own bin folder), but the dev-layout
        // walk-up should find it. Either way the contract is: returns null or an absolute path,
        // never throws.
        var sut = new LauncherPathResolver();

        var result = sut.TryResolve();

        if (result is not null)
        {
            System.IO.Path.IsPathRooted(result).Should().BeTrue();
            result.Should().EndWith(LauncherPathResolver.LauncherFileName);
        }
    }
}
