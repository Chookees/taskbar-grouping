using FluentAssertions;
using Xunit;

namespace TaskbarFolders.Core.Tests;

public class SmokeTests
{
    [Fact]
    public void ProjectLoads()
    {
        true.Should().BeTrue();
    }
}
