using System.IO;
using FluentAssertions;
using TaskbarFolders.Shared.Configuration;
using TaskbarFolders.Shared.Models;
using Xunit;

namespace TaskbarFolders.Core.Tests.Configuration;

public class JsonGroupConfigStoreTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _originalGroupsDir;

    public JsonGroupConfigStoreTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tbf_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        _originalGroupsDir = Shared.Utilities.PathHelper.GroupsDirectory;
    }

    [Fact]
    public async Task SaveAsync_ThenLoadAsync_RoundTrips()
    {
        var store = new TestableGroupConfigStore(_tempDir);
        var config = new GroupConfig
        {
            Id = "test1",
            GroupName = "Dev Tools",
            Columns = 3,
            Apps =
            [
                new AppEntry { Name = "VS Code", Path = @"C:\Code.exe" },
            ],
        };

        await store.SaveAsync(config);
        GroupConfig? loaded = await store.LoadAsync("test1");

        loaded.Should().NotBeNull();
        loaded!.GroupName.Should().Be("Dev Tools");
        loaded.Apps.Should().HaveCount(1);
        loaded.Apps[0].Name.Should().Be("VS Code");
    }

    [Fact]
    public async Task LoadAllAsync_ReturnsAllSavedConfigs()
    {
        var store = new TestableGroupConfigStore(_tempDir);
        await store.SaveAsync(new GroupConfig { Id = "g1", GroupName = "Group 1" });
        await store.SaveAsync(new GroupConfig { Id = "g2", GroupName = "Group 2" });

        IReadOnlyList<GroupConfig> all = await store.LoadAllAsync();

        all.Should().HaveCount(2);
    }

    [Fact]
    public async Task LoadAsync_NonExistentId_ReturnsNull()
    {
        var store = new TestableGroupConfigStore(_tempDir);

        GroupConfig? result = await store.LoadAsync("nonexistent");

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_RemovesConfig()
    {
        var store = new TestableGroupConfigStore(_tempDir);
        await store.SaveAsync(new GroupConfig { Id = "del1", GroupName = "To Delete" });

        await store.DeleteAsync("del1");
        GroupConfig? result = await store.LoadAsync("del1");

        result.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_WithNullConfig_ThrowsArgumentNullException()
    {
        var store = new TestableGroupConfigStore(_tempDir);

        var act = () => store.SaveAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private sealed class TestableGroupConfigStore : IGroupConfigStore
    {
        private readonly string _dir;

        private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
        };

        public TestableGroupConfigStore(string directory)
        {
            _dir = directory;
            Directory.CreateDirectory(_dir);
        }

        public async Task<IReadOnlyList<GroupConfig>> LoadAllAsync()
        {
            if (!Directory.Exists(_dir))
                return [];

            var configs = new List<GroupConfig>();
            foreach (string file in Directory.GetFiles(_dir, "*.json"))
            {
                string json = await File.ReadAllTextAsync(file);
                GroupConfig? config = System.Text.Json.JsonSerializer.Deserialize<GroupConfig>(json, JsonOptions);
                if (config is not null)
                    configs.Add(config);
            }
            return configs;
        }

        public async Task<GroupConfig?> LoadAsync(string groupId)
        {
            string path = Path.Combine(_dir, $"{groupId}.json");
            if (!File.Exists(path))
                return null;

            string json = await File.ReadAllTextAsync(path);
            return System.Text.Json.JsonSerializer.Deserialize<GroupConfig>(json, JsonOptions);
        }

        public async Task SaveAsync(GroupConfig config)
        {
            ArgumentNullException.ThrowIfNull(config);
            string path = Path.Combine(_dir, $"{config.Id}.json");
            string json = System.Text.Json.JsonSerializer.Serialize(config, JsonOptions);
            await File.WriteAllTextAsync(path, json);
        }

        public Task DeleteAsync(string groupId)
        {
            string path = Path.Combine(_dir, $"{groupId}.json");
            if (File.Exists(path))
                File.Delete(path);
            return Task.CompletedTask;
        }
    }
}
