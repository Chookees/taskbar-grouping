using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TaskbarFolders.Core.Icons;
using TaskbarFolders.Launcher.Services;
using TaskbarFolders.Launcher.ViewModels;
using TaskbarFolders.Launcher.Views;
using TaskbarFolders.Shared.Configuration;

namespace TaskbarFolders.Launcher;

/// <summary>
/// Application entry point for the TaskbarFolders Launcher.
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    /// <inheritdoc />
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        string? groupId = ParseGroupId(e.Args);
        if (string.IsNullOrEmpty(groupId))
        {
            Shutdown(1);
            return;
        }

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var viewModel = _serviceProvider.GetRequiredService<PopupViewModel>();
        await viewModel.LoadGroupAsync(groupId).ConfigureAwait(true);

        if (viewModel.Apps.Count == 0)
        {
            Shutdown(1);
            return;
        }

        var window = new PopupWindow(viewModel);
        window.Show();
    }

    /// <inheritdoc />
    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IGroupConfigStore, JsonGroupConfigStore>();
        services.AddSingleton<IIconExtractor, ShellIconExtractor>();
        services.AddSingleton<ProcessLauncher>();
        services.AddTransient<PopupViewModel>();
    }

    private static string? ParseGroupId(string[] args)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals("--group-id", StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        }

        return args.Length > 0 ? args[0] : null;
    }
}
