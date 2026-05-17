using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using TaskbarFolders.Core.Icons;
using TaskbarFolders.Launcher.Configuration;
using TaskbarFolders.Launcher.Services;
using TaskbarFolders.Launcher.ViewModels;
using TaskbarFolders.Launcher.Views;
using TaskbarFolders.Shared.Configuration;

namespace TaskbarFolders.Launcher;

/// <summary>
/// Centralised service-registration helper. Extracted from <see cref="App.OnStartup"/> so
/// the same registration graph can be exercised by composition tests without parsing CLI
/// args or spawning real processes.
/// </summary>
[SupportedOSPlatform("windows")]
public static class LauncherServiceCollectionExtensions
{
    /// <summary>
    /// Registers every Launcher-side service, view model, and view used by the running app.
    /// Caller supplies <paramref name="options"/> + <paramref name="paths"/> so production
    /// (which derives these from the command line and <c>%APPDATA%</c>) and tests (which
    /// inject fixtures) share the same registration list.
    /// </summary>
    public static IServiceCollection AddTaskbarFoldersLauncher(
        this IServiceCollection services,
        LauncherOptions options,
        IAppDataPathProvider paths)
    {
        services.AddSingleton(options);
        services.AddSingleton(paths);

        // Persistence — the launcher is read-only against group configs and settings.
        services.AddSingleton<IGroupConfigStore, JsonGroupConfigStore>();
        services.AddSingleton<IAppSettingsStore, JsonAppSettingsStore>();

        // Icon engine — needed for the popup tiles.
        services.AddSingleton<IIconExtractor, ShellIconExtractor>();
        services.AddSingleton<IIconCache, FileSystemIconCache>();

        // Launcher-only services.
        services.AddSingleton<IProcessLauncher, ProcessLauncher>();
        services.AddSingleton<ICursorAnchor, LauncherCursorAnchor>();
        services.AddSingleton<ITaskbarPositionHelper, TaskbarPositionHelper>();

        services.AddSingleton<PopupViewModel>();
        services.AddTransient<PopupWindow>();

        return services;
    }
}
