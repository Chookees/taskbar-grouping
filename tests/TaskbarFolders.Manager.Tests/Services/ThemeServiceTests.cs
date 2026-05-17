using FluentAssertions;
using Moq;
using TaskbarFolders.Manager.Services;
using TaskbarFolders.Shared.Models;
using Xunit;

namespace TaskbarFolders.Manager.Tests.Services;

public class ThemeServiceTests
{
    private static Mock<ISystemThemeProbe> Probe(bool isLight)
    {
        var mock = new Mock<ISystemThemeProbe>();
        mock.SetupGet(p => p.IsLightMode).Returns(isLight);
        return mock;
    }

    [Fact]
    public void DefaultPreference_IsSystem()
    {
        var sut = new ThemeService(Probe(isLight: true).Object);
        sut.Preference.Should().Be(ThemePreference.System);
    }

    [Theory]
    [InlineData(true, ThemePreference.Light)]
    [InlineData(false, ThemePreference.Dark)]
    public void EffectiveTheme_FromSystem_FollowsProbe(bool isLight, ThemePreference expected)
    {
        var sut = new ThemeService(Probe(isLight).Object);
        sut.SetPreference(ThemePreference.System);

        sut.EffectiveTheme.Should().Be(expected);
    }

    [Theory]
    [InlineData(ThemePreference.Light)]
    [InlineData(ThemePreference.Dark)]
    public void EffectiveTheme_FromExplicitPreference_IgnoresProbe(ThemePreference preference)
    {
        // Probe says "light" but explicit preference must win.
        var sut = new ThemeService(Probe(isLight: true).Object);
        sut.SetPreference(preference);

        sut.EffectiveTheme.Should().Be(preference);
    }

    [Fact]
    public void SetPreference_UpdatesPreference()
    {
        var sut = new ThemeService(Probe(isLight: true).Object);

        sut.SetPreference(ThemePreference.Dark);

        sut.Preference.Should().Be(ThemePreference.Dark);
    }

    [Fact]
    public void Dispose_IsSafeToCallRepeatedly()
    {
        var sut = new ThemeService(Probe(isLight: true).Object);
        sut.SetPreference(ThemePreference.System); // wires the listener

        sut.Dispose();
        var act = () => sut.Dispose();

        act.Should().NotThrow();
    }
}
