using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskbarFolders.Launcher.Configuration;
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
    protected override void OnStartup(StartupEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

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

        services.AddTransient<PopupWindow>();
        _services = services.BuildServiceProvider();

        _services.GetRequiredService<ILogger<App>>()
            .LogInformation("Launcher starting for group {GroupId}.", groupId);

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
