using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskbarFolders.Core.Icons;
using TaskbarFolders.Launcher.Configuration;
using TaskbarFolders.Launcher.Services;
using TaskbarFolders.Launcher.ViewModels;
using TaskbarFolders.Launcher.Views;
using TaskbarFolders.Shared.Configuration;
using TaskbarFolders.Shared.Logging;

namespace TaskbarFolders.Launcher;

/// <summary>
/// Application entry point for the TaskbarFolders Launcher.
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _services;

    /// <summary>
    /// Gets the application service provider once <see cref="OnStartup"/> has completed.
    /// </summary>
    public IServiceProvider? Services => _services;

    /// <inheritdoc/>
    protected override async void OnStartup(StartupEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        // Per-monitor DPI awareness must be set before any HWND is created so all popups
        // render at the right scaling on mixed-DPI multi-monitor setups. Falling back
        // silently is fine — Windows then treats us as system-DPI-aware (still correct
        // on single-monitor or uniform-DPI rigs).
        try
        {
            Interop.NativeMethods.SetProcessDpiAwarenessContext(
                Interop.NativeMethods.DpiAwarenessContextPerMonitorAwareV2);
        }
        catch (System.EntryPointNotFoundException)
        {
            // Pre-1703 Windows — leave DPI awareness at the manifest default.
        }

        var groupId = CommandLineParser.TryParseGroupId(e.Args);
        if (groupId is null)
        {
            // No DI/logger yet — emit a trace so dev/QA can see this in a debugger; M4 will
            // additionally surface this as a user-visible toast.
            Trace.TraceError("Launcher started without required {0} argument.", CommandLineParser.GroupIdArg);
            Shutdown(1);
            return;
        }

        var paths = new AppDataPathProvider();

        var services = new ServiceCollection();
        services.AddLogging(logging => logging.AddTaskbarFoldersFileLogging(paths.LogsDirectory, "launcher"));
        services.AddSingleton(new LauncherOptions(groupId));

        // Persistence — the launcher is read-only against group configs and settings.
        services.AddSingleton<IAppDataPathProvider>(paths);
        services.AddSingleton<IGroupConfigStore, JsonGroupConfigStore>();
        services.AddSingleton<IAppSettingsStore, JsonAppSettingsStore>();

        // Icon engine — needed for the popup tiles.
        services.AddSingleton<IIconExtractor, ShellIconExtractor>();
        services.AddSingleton<IIconCache, FileSystemIconCache>();

        // Launcher-only services.
        services.AddSingleton<IProcessLauncher, ProcessLauncher>();
        services.AddSingleton<ITaskbarPositionHelper, TaskbarPositionHelper>();

        services.AddSingleton<PopupViewModel>();
        services.AddTransient<PopupWindow>();
        _services = services.BuildServiceProvider();

        _services.GetRequiredService<ILogger<App>>()
            .LogInformation("Launcher starting for group {GroupId}.", groupId);

        // Apply the persisted theme before the window is built so DynamicResource bindings
        // paint correctly on the first frame.
        var settings = await _services.GetRequiredService<IAppSettingsStore>().LoadAsync().ConfigureAwait(true);
        LauncherThemeApplier.Apply(this, settings.Theme);

        // Hydrate the view model before the window builds so the icon grid paints in one frame.
        await _services.GetRequiredService<PopupViewModel>().LoadAsync().ConfigureAwait(true);

        var popup = _services.GetRequiredService<PopupWindow>();
        MainWindow = popup;
        popup.Show();

        base.OnStartup(e);
    }

    /// <inheritdoc/>
    protected override void OnExit(ExitEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        _services?.Dispose();
        _services = null;

        base.OnExit(e);
    }
}
