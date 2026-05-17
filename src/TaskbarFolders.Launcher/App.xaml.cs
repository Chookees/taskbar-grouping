using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TaskbarFolders.Launcher.Configuration;
using TaskbarFolders.Launcher.Views;
using TaskbarFolders.Shared.Configuration;

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
            // Started without the required argument (e.g. manual double-click without context).
            // M4 replaces this with a user-visible toast; until logging lands in M1.6 we trace and exit.
            Trace.TraceError("Launcher started without required {0} argument.", CommandLineParser.GroupIdArg);
            Shutdown(1);
            return;
        }

        var services = new ServiceCollection();
        services.AddSingleton(new LauncherOptions(groupId));

        // Persistence — the launcher is read-only against group configs and settings.
        services.AddSingleton<IAppDataPathProvider, AppDataPathProvider>();
        services.AddSingleton<IGroupConfigStore, JsonGroupConfigStore>();
        services.AddSingleton<IAppSettingsStore, JsonAppSettingsStore>();

        services.AddTransient<PopupWindow>();
        _services = services.BuildServiceProvider();

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
