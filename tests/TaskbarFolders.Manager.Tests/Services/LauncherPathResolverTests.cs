using System;
using System.IO;
using FluentAssertions;
using TaskbarFolders.Manager.Services;
using Xunit;

namespace TaskbarFolders.Manager.Tests.Services;

public sealed class LauncherPathResolverTests : IDisposable
{
    private readonly string _tempRoot;

    public LauncherPathResolverTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "TaskbarFolders.Resolver." + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

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
            Path.IsPathRooted(result).Should().BeTrue();
            result.Should().EndWith(LauncherPathResolver.LauncherFileName);
        }
    }

    [Fact]
    public void TryResolveFrom_FindsLauncherInSiblingFolder_MatchingInstallerLayout()
    {
        // Reproduce the installer / portable ZIP layout: Manager and Launcher live as
        // sibling folders under the install root, not in the same directory. The regression
        // this guards is the v0.2.0 "Show shortcut..." silent failure where TryResolve only
        // checked the side-by-side path and returned null on every real-world install.
        var managerDir = Path.Combine(_tempRoot, "Manager");
        var launcherDir = Path.Combine(_tempRoot, "Launcher");
        Directory.CreateDirectory(managerDir);
        Directory.CreateDirectory(launcherDir);

        var launcherExe = Path.Combine(launcherDir, LauncherPathResolver.LauncherFileName);
        File.WriteAllBytes(launcherExe, [0x4D, 0x5A]); // "MZ" — content is irrelevant, only File.Exists matters.

        var sut = new LauncherPathResolver();
        var result = sut.TryResolveFrom(managerDir);

        result.Should().NotBeNull();
        Path.GetFullPath(result!).Should().Be(Path.GetFullPath(launcherExe));
    }

    [Fact]
    public void TryResolveFrom_PrefersSideBySide_OverSiblingFolder()
    {
        // If a future packaging ever ships both binaries in one folder, the side-by-side probe
        // (strategy 1) must win over the sibling probe (strategy 2). Plant both, expect strategy 1.
        var managerDir = Path.Combine(_tempRoot, "Manager");
        var launcherDir = Path.Combine(_tempRoot, "Launcher");
        Directory.CreateDirectory(managerDir);
        Directory.CreateDirectory(launcherDir);

        var sideBySide = Path.Combine(managerDir, LauncherPathResolver.LauncherFileName);
        var sibling = Path.Combine(launcherDir, LauncherPathResolver.LauncherFileName);
        File.WriteAllBytes(sideBySide, [0x4D, 0x5A]);
        File.WriteAllBytes(sibling, [0x4D, 0x5A]);

        var sut = new LauncherPathResolver();
        var result = sut.TryResolveFrom(managerDir);

        result.Should().Be(sideBySide);
    }

    [Fact]
    public void TryResolveFrom_FindsLauncher_WhenDevTreeFrameworkDiffersFromManager()
    {
        // Dev-tree layout as `dotnet run` produces it: the Manager builds into
        // net8.0-windows, the launcher into net8.0-windows10.0.19041.0 because it needs the
        // WinRT taskbar projections. The walk-up probe used to reuse the Manager's framework
        // segment, so it looked for the launcher under net8.0-windows, never matched, and
        // GroupSyncService aborted before writing any icon or shortcut - no group created from
        // a dev run was ever pinnable.
        File.WriteAllText(Path.Combine(_tempRoot, "TaskbarFolders.sln"), string.Empty);

        var managerBin = Path.Combine(_tempRoot, "src", "TaskbarFolders.Manager", "bin", "Release", "net8.0-windows");
        var launcherBin = Path.Combine(_tempRoot, "src", "TaskbarFolders.Launcher", "bin", "Release", "net8.0-windows10.0.19041.0");
        Directory.CreateDirectory(managerBin);
        Directory.CreateDirectory(launcherBin);

        var launcherExe = Path.Combine(launcherBin, LauncherPathResolver.LauncherFileName);
        File.WriteAllBytes(launcherExe, [0x4D, 0x5A]);

        var sut = new LauncherPathResolver();
        var result = sut.TryResolveFrom(managerBin);

        result.Should().NotBeNull();
        Path.GetFullPath(result!).Should().Be(Path.GetFullPath(launcherExe));
    }

    [Fact]
    public void TryResolveFrom_KeepsTheConfiguration_OfTheCallingManager()
    {
        // Enumerating framework folders must not start crossing configurations: a Debug Manager
        // has to launch the Debug launcher even when a Release build is also on disk.
        File.WriteAllText(Path.Combine(_tempRoot, "TaskbarFolders.sln"), string.Empty);

        var managerBin = Path.Combine(_tempRoot, "src", "TaskbarFolders.Manager", "bin", "Debug", "net8.0-windows");
        Directory.CreateDirectory(managerBin);

        var launcherRoot = Path.Combine(_tempRoot, "src", "TaskbarFolders.Launcher", "bin");
        var debugExe = Path.Combine(launcherRoot, "Debug", "net8.0-windows10.0.19041.0", LauncherPathResolver.LauncherFileName);
        var releaseExe = Path.Combine(launcherRoot, "Release", "net8.0-windows10.0.19041.0", LauncherPathResolver.LauncherFileName);
        Directory.CreateDirectory(Path.GetDirectoryName(debugExe)!);
        Directory.CreateDirectory(Path.GetDirectoryName(releaseExe)!);
        File.WriteAllBytes(debugExe, [0x4D, 0x5A]);
        File.WriteAllBytes(releaseExe, [0x4D, 0x5A]);

        var sut = new LauncherPathResolver();
        var result = sut.TryResolveFrom(managerBin);

        Path.GetFullPath(result!).Should().Be(Path.GetFullPath(debugExe));
    }

    [Fact]
    public void TryResolveFrom_ReturnsNull_WhenNoLayoutMatches()
    {
        // No launcher anywhere — none of the three probes find a file. Contract: null, no throw.
        var managerDir = Path.Combine(_tempRoot, "Manager");
        Directory.CreateDirectory(managerDir);

        var sut = new LauncherPathResolver();
        var result = sut.TryResolveFrom(managerDir);

        result.Should().BeNull();
    }

    [Fact]
    public void TryResolveFrom_RejectsBlankBaseDirectory()
    {
        var sut = new LauncherPathResolver();

        var act = () => sut.TryResolveFrom("");

        act.Should().Throw<ArgumentException>();
    }
}
