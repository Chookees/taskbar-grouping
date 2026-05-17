using System;
using System.Windows;
using FluentAssertions;
using TaskbarFolders.Launcher.Services;
using Xunit;

namespace TaskbarFolders.Launcher.Tests.Services;

public class CursorAnchorTests
{
    [Fact]
    public void Position_BeforeSeed_Throws()
    {
        var sut = new LauncherCursorAnchor();

        var act = () => _ = sut.Position;

        act.Should().Throw<InvalidOperationException>("reading before seeding indicates a startup-order bug");
    }

    [Fact]
    public void Seed_ThenPosition_RoundTripsValue()
    {
        var sut = new LauncherCursorAnchor();

        sut.Seed(new Point(123.5, 456.25));

        sut.Position.Should().Be(new Point(123.5, 456.25));
    }

    [Fact]
    public void Seed_Twice_LastWriteWins()
    {
        // Repeated seeds are not an error — e.g. a future hot-restart path could re-seed.
        // Contract is last-write-wins, matching the most recent click intent.
        var sut = new LauncherCursorAnchor();
        sut.Seed(new Point(1, 1));
        sut.Seed(new Point(2, 2));

        sut.Position.Should().Be(new Point(2, 2));
    }
}
