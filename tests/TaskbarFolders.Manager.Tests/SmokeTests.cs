using FluentAssertions;
using Xunit;

namespace TaskbarFolders.Manager.Tests;

public class SmokeTests
{
    [Fact]
    public void ProjectLoads()
    {
        true.Should().BeTrue();
    }
}
