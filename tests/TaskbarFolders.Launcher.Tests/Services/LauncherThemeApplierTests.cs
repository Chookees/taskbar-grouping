using FluentAssertions;
using TaskbarFolders.Launcher.Services;
using TaskbarFolders.Shared.Models;
using Xunit;

namespace TaskbarFolders.Launcher.Tests.Services;

public class LauncherThemeApplierTests
{
    [Theory]
    [InlineData(ThemePreference.Light, ThemePreference.Light)]
    [InlineData(ThemePreference.Dark, ThemePreference.Dark)]
    public void Resolve_ExplicitPreference_ReturnsPreferenceVerbatim(ThemePreference input, ThemePreference expected)
    {
        LauncherThemeApplier.Resolve(input).Should().Be(expected);
    }

    [Fact]
    public void Resolve_System_ReturnsLightOrDarkBasedOnCurrentRegistry()
    {
        // Cannot assert a specific value without writing to the registry — this only confirms
        // System never propagates back as a result; the resolver always commits to Light or Dark.
        var result = LauncherThemeApplier.Resolve(ThemePreference.System);
        result.Should().BeOneOf(ThemePreference.Light, ThemePreference.Dark);
    }
}
