using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using TaskbarFolders.Shared.Configuration;
using TaskbarFolders.Shared.Models;
using Xunit;

namespace TaskbarFolders.Core.Tests.Configuration;

public sealed class JsonAppSettingsStoreTests : IDisposable
{
    private readonly string _tempBase;
    private readonly AppDataPathProvider _paths;
    private readonly JsonAppSettingsStore _sut;

    public JsonAppSettingsStoreTests()
    {
        _tempBase = Path.Combine(Path.GetTempPath(), "TaskbarFolders.Test." + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempBase);

        _paths = new AppDataPathProvider(_tempBase);
        _sut = new JsonAppSettingsStore(_paths);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempBase))
        {
            Directory.Delete(_tempBase, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task LoadAsync_ReturnsDefaults_WhenFileMissing()
    {
        var result = await _sut.LoadAsync();

        result.Should().NotBeNull();
        result.Theme.Should().Be(ThemePreference.System);
        result.PopupPosition.Should().Be(PopupPositionPreference.Auto);
        result.AutoStart.Should().BeFalse();
        result.EnableAnimations.Should().BeTrue();
    }

    [Fact]
    public async Task SaveThenLoad_RoundTripsAllFields()
    {
        var input = new AppSettings
        {
            AutoStart = true,
            Theme = ThemePreference.Dark,
            EnableAnimations = false,
            PopupPosition = PopupPositionPreference.Above,
        };

        await _sut.SaveAsync(input);
        var loaded = await _sut.LoadAsync();

        loaded.AutoStart.Should().BeTrue();
        loaded.Theme.Should().Be(ThemePreference.Dark);
        loaded.EnableAnimations.Should().BeFalse();
        loaded.PopupPosition.Should().Be(PopupPositionPreference.Above);
    }

    [Fact]
    public async Task SaveAsync_OverwritesExistingFile()
    {
        await _sut.SaveAsync(new AppSettings { Theme = ThemePreference.Light });
        await _sut.SaveAsync(new AppSettings { Theme = ThemePreference.Dark });

        var loaded = await _sut.LoadAsync();

        loaded.Theme.Should().Be(ThemePreference.Dark);
    }

    [Fact]
    public async Task SaveAsync_LeavesNoTempFileBehindOnSuccess()
    {
        await _sut.SaveAsync(new AppSettings { Theme = ThemePreference.System });

        var leftover = Path.Combine(_paths.AppDataRoot, "settings.json.tmp");
        File.Exists(leftover).Should().BeFalse();
    }
}
