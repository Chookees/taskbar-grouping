using System.IO;
using System.Text.Json;
using FluentAssertions;
using TaskbarFolders.Shared.Models;
using Xunit;

namespace TaskbarFolders.Core.Tests.Configuration;

public class JsonAppSettingsStoreTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _settingsPath;

    public JsonAppSettingsStoreTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tbf_settings_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _settingsPath = Path.Combine(_tempDir, "settings.json");
    }

    [Fact]
    public async Task Load_WhenFileNotExists_ReturnsDefaults()
    {
        AppSettings result = await LoadAsync();

        result.Should().NotBeNull();
        result.AutoStart.Should().BeFalse();
        result.Theme.Should().Be("system");
        result.EnableAnimations.Should().BeTrue();
        result.PopupPosition.Should().Be("auto");
    }

    [Fact]
    public async Task SaveThenLoad_RoundTrips()
    {
        var settings = new AppSettings
        {
            AutoStart = true,
            Theme = "dark",
            EnableAnimations = false,
            PopupPosition = "above",
        };

        await SaveAsync(settings);
        AppSettings loaded = await LoadAsync();

        loaded.AutoStart.Should().BeTrue();
        loaded.Theme.Should().Be("dark");
        loaded.EnableAnimations.Should().BeFalse();
        loaded.PopupPosition.Should().Be("above");
    }

    [Fact]
    public async Task Save_CreatesValidJson()
    {
        var settings = new AppSettings { Theme = "light" };

        await SaveAsync(settings);
        string json = await File.ReadAllTextAsync(_settingsPath);

        json.Should().Contain("\"theme\"");
        json.Should().Contain("\"light\"");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private async Task<AppSettings> LoadAsync()
    {
        if (!File.Exists(_settingsPath))
            return new AppSettings();

        string json = await File.ReadAllTextAsync(_settingsPath);
        return JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        }) ?? new AppSettings();
    }

    private async Task SaveAsync(AppSettings settings)
    {
        string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
        await File.WriteAllTextAsync(_settingsPath, json);
    }
}
