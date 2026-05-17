using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskbarFolders.Core.Icons;
using TaskbarFolders.Manager.Services;
using TaskbarFolders.Manager.ViewModels;
using TaskbarFolders.Manager.Views;
using TaskbarFolders.Shared.Configuration;
using TaskbarFolders.Shared.Logging;

namespace TaskbarFolders.Manager;

/// <summary>
/// Application entry point for the TaskbarFolders Manager.
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class
    /// and constructs the generic host with the service container.
    /// </summary>
    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureLogging((_, logging) =>
            {
                var paths = new AppDataPathProvider();
                logging.AddTaskbarFoldersFileLogging(paths.LogsDirectory, "manager");
            })
            .ConfigureServices(ConfigureServices)
            .Build();
    }

    /// <summary>
    /// Gets the application service provider. Available after construction.
    /// </summary>
    public IServiceProvider Services => _host.Services;

    /// <inheritdoc/>
    protected override async void OnStartup(StartupEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        await _host.StartAsync().ConfigureAwait(true);

        // Load groups before constructing the window so the sidebar binding shows data
        // immediately rather than flashing empty for one frame.
        var viewModel = _host.Services.GetRequiredService<MainWindowViewModel>();
        await viewModel.LoadGroupsAsync().ConfigureAwait(true);

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        MainWindow = mainWindow;
        mainWindow.Show();

        base.OnStartup(e);
    }

    /// <inheritdoc/>
    protected override async void OnExit(ExitEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        await _host.StopAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(true);
        _host.Dispose();

        base.OnExit(e);
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // Persistence — singletons because the stores carry no per-call state and the
        // path provider is rooted at %APPDATA% for the lifetime of the process.
        services.AddSingleton<IAppDataPathProvider, AppDataPathProvider>();
        services.AddSingleton<IGroupConfigStore, JsonGroupConfigStore>();
        services.AddSingleton<IAppSettingsStore, JsonAppSettingsStore>();

        // Icon engine — singletons; ShellIconExtractor is stateless and the cache (M2.4)
        // will replace IIconExtractor with a caching decorator.
        services.AddSingleton<IIconExtractor, ShellIconExtractor>();
        services.AddSingleton<ICompositeIconGenerator, CompositeIconGenerator>();
        services.AddSingleton<IIcoFileWriter, IcoFileWriter>();
        services.AddSingleton<IIconCache, FileSystemIconCache>();

        // Manager-side services.
        services.AddSingleton<IAutoStartService, RegistryAutoStartService>();

        // View models — MainWindow is itself a singleton conceptually (one main window per process),
        // so the backing VM is singleton too. App.OnStartup loads groups into it before showing the window.
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<GroupEditorViewModel>();
        services.AddTransient<SettingsViewModel>();

        // Views — transient so each Show creates a fresh window instance.
        services.AddTransient<MainWindow>();
        services.AddTransient<SettingsWindow>();
    }
}
