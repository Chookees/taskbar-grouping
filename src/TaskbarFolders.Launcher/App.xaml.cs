using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskbarFolders.Core.Shortcuts;
using TaskbarFolders.Launcher.Configuration;
using TaskbarFolders.Launcher.Services;
using TaskbarFolders.Launcher.ViewModels;
using TaskbarFolders.Shared.Configuration;
using TaskbarFolders.Shared.Logging;
using TaskbarFolders.Shared.Models;

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

        // Capture the cursor position FIRST, before anything else can take 100+ ms. This is
        // the click location — by the time WPF has bootstrapped (~300–500 ms) the cursor has
        // typically drifted, which is why the v0.2 helper's late GetCursorPos call produced
        // visually-random popup placement. GetCursorPos is a single user32 syscall, no
        // dependencies on DPI, COM, or WPF state. Seeded into ICursorAnchor below once DI
        // has been built.
        //
        // GetCursorPos can fail on restricted desktops / session 0. Out param is default-
        // initialised to (0, 0) on failure, which would silently anchor the popup at the
        // top-left of the primary monitor. Detect the failure and fall back to the screen
        // centre so the user sees the popup near the middle instead of jammed in a corner.
        System.Windows.Point anchor;
        if (Interop.NativeMethods.GetCursorPos(out var clickPoint))
        {
            anchor = new System.Windows.Point(clickPoint.X, clickPoint.Y);
        }
        else
        {
            anchor = new System.Windows.Point(
                System.Windows.SystemParameters.PrimaryScreenWidth / 2,
                System.Windows.SystemParameters.PrimaryScreenHeight / 2);
        }

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

        // Stamp the process AUMID BEFORE any window is created so Windows matches the
        // popup to the pinned .lnk tile (which carries the same AUMID via PKEY_AppUserModel_ID).
        // Without this the taskbar would show a second "ghost" entry for the running process.
        // The HRESULT is discarded: a non-zero result here means very old Windows or a
        // restricted token, neither of which is fatal — the popup still works, just without
        // grouping under the pinned tile.
        _ = Interop.NativeMethods.SetCurrentProcessExplicitAppUserModelID(GroupAumid.For(groupId));

        var paths = new AppDataPathProvider();

        // Load settings BEFORE the DI container is built so the resolved AppSettings instance
        // can be registered as a singleton. Replaces the v0.2 pattern that loaded settings
        // twice — once here, once again inside PopupWindow.PositionAndConfigureAsync.
        var settings = await new JsonAppSettingsStore(paths).LoadAsync().ConfigureAwait(true);

        var services = new ServiceCollection();
        services.AddLogging(logging => logging.AddTaskbarFoldersFileLogging(paths.LogsDirectory, "launcher"));
        services.AddTaskbarFoldersLauncher(new LauncherOptions(groupId), paths);
        services.AddSingleton(settings);
        // ValidateOnBuild/ValidateScopes match the production composition tests so any DI
        // regression fails at process start rather than at first GetService.
        _services = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        });

        // Seed the cursor anchor BEFORE PopupViewModel/PopupWindow are resolved so any
        // placement lookup during the first paint sees a populated value.
        _services.GetRequiredService<ICursorAnchor>().Seed(anchor);

        _services.GetRequiredService<ILogger<App>>()
            .LogInformation("Launcher starting for group {GroupId}.", groupId);

        // Apply the persisted theme before the window is built so DynamicResource bindings
        // paint correctly on the first frame.
        LauncherThemeApplier.Apply(this, settings.Theme);

        // Two-phase load: metadata first (group name, columns, app names — ~5 ms) so the
        // window can paint immediately, then per-app icon extraction in the background.
        // Pre-v0.3 this awaited the full icon-extraction pipeline before Show(), which froze
        // the UI for 200 ms–3 s on cold cache. The window now appears within ~50 ms; icons
        // stream in as they resolve.
        var viewModel = _services.GetRequiredService<PopupViewModel>();
        await viewModel.LoadAsync().ConfigureAwait(true);

        var popup = _services.GetRequiredService<Views.PopupWindow>();
        MainWindow = popup;
        popup.Show();

        viewModel.StartIconLoad();

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
