using FluentAssertions;
using Xunit;

namespace TaskbarFolders.Launcher.Tests;

public class SmokeTests
{
    [Fact]
    public void ProjectLoads()
    {
        true.Should().BeTrue();
    }
}
