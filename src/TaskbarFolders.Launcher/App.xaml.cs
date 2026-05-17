using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskbarFolders.Core.Icons;
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
/// <remarks>
/// Two modes:
/// <list type="bullet">
///   <item><b>Popup mode</b> (default) — user clicked a pinned tile; show the apps grid.</item>
///   <item><b>Pin mode</b> (<c>--pin-mode</c>) — Manager asked us to pin the group to the
///   taskbar via <see cref="Windows.UI.Shell.TaskbarManager"/>. Shows the system permission
///   dialog and exits with a status code the Manager interprets.</item>
/// </list>
/// </remarks>
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

        var (groupId, fromAumid) = ResolveGroupId(e.Args);
        if (groupId is null)
        {
            Trace.TraceError(
                "Launcher started without {0} argument and no AUMID fallback available.",
                CommandLineParser.GroupIdArg);
            Shutdown(1);
            return;
        }

        // Any unhandled exception inside the async branches would otherwise crash the
        // process via async-void with an unobservable exit code; the Manager would map
        // the resulting random exit code to PinResult.Error. Wrap so we always shutdown
        // with a documented exit code.
        try
        {
            if (CommandLineParser.HasPinMode(e.Args))
            {
                await RunPinModeAsync(groupId, fromAumid).ConfigureAwait(true);
                return;
            }

            await RunPopupModeAsync(e, groupId, fromAumid).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Trace.TraceError("Launcher OnStartup threw: {0}", ex);
            Shutdown(3);
        }
    }

    /// <summary>
    /// Resolves the target group id from either the explicit <c>--group-id</c> argument
    /// (Manager-spawned + .lnk-pinned paths) or the AUMID Windows already assigned to the
    /// process (TaskbarManager-pinned tile paths where the original command line is not
    /// preserved). Returns a tuple of the id and whether it was recovered from the AUMID;
    /// callers use the latter to decide whether to re-stamp the AUMID (re-stamping an
    /// inherited AUMID can cause identity drift if Windows normalised the string).
    /// </summary>
    private static (string? GroupId, bool FromAumid) ResolveGroupId(string[] args)
    {
        var fromArgs = CommandLineParser.TryParseGroupId(args);
        if (fromArgs is not null)
        {
            return (fromArgs, false);
        }

        var assignedAumid = Interop.NativeMethods.TryGetCurrentProcessAumid();
        if (assignedAumid is not null && GroupAumid.TryExtractGroupId(assignedAumid, out var fromAumid))
        {
            return (fromAumid, true);
        }

        return (null, false);
    }

    /// <summary>
    /// Pin-mode entry point. Stamps AUMID, builds a minimal DI scope, shows the off-screen
    /// host window so the WinRT pin dialog has a foreground parent, awaits the pin runner,
    /// shuts down with the runner's exit code so the Manager can react to the outcome.
    /// </summary>
    private async Task RunPinModeAsync(string groupId, bool aumidAlreadyInherited)
    {
        // AUMID must match GroupAumid.For(groupId) before the WinRT call so
        // RequestPinCurrentAppAsync pins the right identity. Skip the stamp when Windows
        // already gave us the AUMID via process activation — re-stamping the inherited
        // string could drift the identity if Windows normalised the case or trimmed
        // whitespace differently from our For() formatter.
        if (!aumidAlreadyInherited)
        {
            _ = Interop.NativeMethods.SetCurrentProcessExplicitAppUserModelID(GroupAumid.For(groupId));
        }

        var paths = new AppDataPathProvider();
        var services = new ServiceCollection();
        services.AddLogging(logging => logging.AddTaskbarFoldersFileLogging(paths.LogsDirectory, "launcher"));
        services.AddTaskbarFoldersLauncher(new LauncherOptions(groupId), paths);
        // PopupWindow is registered in the same graph but never resolved in pin-mode;
        // AppSettings is required by the DI graph regardless, so register a default.
        services.AddSingleton(new AppSettings());

        _services = services.BuildServiceProvider();

        var logger = _services.GetRequiredService<ILogger<App>>();
        logger.LogInformation("Launcher pin-mode starting for group {GroupId}.", groupId);

        var host = _services.GetRequiredService<Views.PinHostWindow>();
        MainWindow = host;
        host.Show();
        host.Activate();

        var runner = _services.GetRequiredService<TaskbarPinRunner>();
        var exitCode = await runner.RunAsync(host).ConfigureAwait(true);

        logger.LogInformation("Pin runner exit code {ExitCode} for group {GroupId}.", exitCode, groupId);

        host.Close();
        Shutdown(exitCode);
    }

    /// <summary>
    /// Popup-mode entry point. Existing v0.3 startup path, factored out to keep
    /// <see cref="OnStartup"/> a thin dispatcher.
    /// </summary>
    private async Task RunPopupModeAsync(StartupEventArgs e, string groupId, bool aumidAlreadyInherited)
    {
        // v0.4.1: per-checkpoint Stopwatch timestamps so the launcher-*.log shows where the
        // user's perceived popup-open time goes. The single LogInformation line at the end
        // is the only output the user has to look at. GetTimestamp() costs ~10 ns per call;
        // negligible against the 100s of ms we are profiling.
        var tStart = Stopwatch.GetTimestamp();

        // Capture the cursor position FIRST, before anything else can take 100+ ms. This is
        // the click location — by the time WPF has bootstrapped (~300–500 ms) the cursor has
        // typically drifted, which is why the v0.2 helper's late GetCursorPos call produced
        // visually-random popup placement.
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
        // render at the right scaling on mixed-DPI multi-monitor setups.
        try
        {
            Interop.NativeMethods.SetProcessDpiAwarenessContext(
                Interop.NativeMethods.DpiAwarenessContextPerMonitorAwareV2);
        }
        catch (System.EntryPointNotFoundException)
        {
            // Pre-1703 Windows — leave DPI awareness at the manifest default.
        }

        // Stamp the process AUMID BEFORE any window is created so Windows matches the
        // popup to the pinned tile (which carries the same AUMID via PKEY_AppUserModel_ID).
        // Skip when Windows already gave us the AUMID via process activation — see
        // RunPinModeAsync for the identity-drift rationale.
        if (!aumidAlreadyInherited)
        {
            _ = Interop.NativeMethods.SetCurrentProcessExplicitAppUserModelID(GroupAumid.For(groupId));
        }
        var tAumid = Stopwatch.GetTimestamp();

        var paths = new AppDataPathProvider();

        // Load settings BEFORE the DI container is built so the resolved AppSettings instance
        // can be registered as a singleton (v0.3 single-load pattern).
        var settings = await new JsonAppSettingsStore(paths).LoadAsync().ConfigureAwait(true);
        var tSettings = Stopwatch.GetTimestamp();

        var services = new ServiceCollection();
        services.AddLogging(logging => logging.AddTaskbarFoldersFileLogging(paths.LogsDirectory, "launcher"));
        services.AddTaskbarFoldersLauncher(new LauncherOptions(groupId), paths);
        services.AddSingleton(settings);
#if DEBUG
        _services = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        });
#else
        _services = services.BuildServiceProvider();
#endif
        var tDi = Stopwatch.GetTimestamp();

        // Seed the cursor anchor BEFORE PopupViewModel/PopupWindow are resolved so any
        // placement lookup during the first paint sees a populated value.
        _services.GetRequiredService<ICursorAnchor>().Seed(anchor);

        var logger = _services.GetRequiredService<ILogger<App>>();
        logger.LogInformation("Launcher starting for group {GroupId}.", groupId);

        // Apply the persisted theme before the window is built so DynamicResource bindings
        // paint correctly on the first frame.
        LauncherThemeApplier.Apply(this, settings.Theme);
        var tTheme = Stopwatch.GetTimestamp();

        // Two-phase load: metadata first (group name, columns, app names — ~5 ms) so the
        // window can paint immediately, then per-app icon extraction in the background.
        var viewModel = _services.GetRequiredService<PopupViewModel>();
        await viewModel.LoadAsync().ConfigureAwait(true);
        var tVm = Stopwatch.GetTimestamp();

        var popup = _services.GetRequiredService<Views.PopupWindow>();
        MainWindow = popup;
        popup.Show();
        var tShown = Stopwatch.GetTimestamp();

        viewModel.StartIconLoad();

        // Deferred startup IO: prune stale icon-cache PNGs and old log files in the background
        // so the first paint of the popup is not blocked by ~10-70 ms of enumerate-and-delete.
        _services.GetRequiredService<IIconCache>().StartBackgroundPrune();
        _services.GetServices<ILoggerProvider>()
            .OfType<FileLoggerProvider>()
            .FirstOrDefault()?.StartBackgroundPrune();

        // Single timing summary, emitted from the Loaded handler so we also capture the
        // "from process start to user-visible first paint" wall-clock (tLoaded), not just
        // up to Show(). `processAge` is the time from Process.StartTime to tStart —
        // captures the .NET runtime cold-start cost (native-lib extraction, JIT, WPF
        // assembly load) that happens BEFORE RunPopupModeAsync is even called. Without
        // this, the log would mislead into chasing later phases when the dominant cost
        // is often runtime bootstrap.
        var processStart = Process.GetCurrentProcess().StartTime;
        popup.Loaded += (_, _) =>
        {
            var tLoaded = Stopwatch.GetTimestamp();
            var processAge = (DateTime.Now - processStart).TotalMilliseconds;
            logger.LogInformation(
                "Startup timing (ms from tStart, processAge={ProcAge:F0}): aumid={Aumid:F0} settings={Settings:F0} di={Di:F0} theme={Theme:F0} vm={Vm:F0} show={Show:F0} loaded={Loaded:F0}",
                processAge,
                ToMs(tStart, tAumid),
                ToMs(tStart, tSettings),
                ToMs(tStart, tDi),
                ToMs(tStart, tTheme),
                ToMs(tStart, tVm),
                ToMs(tStart, tShown),
                ToMs(tStart, tLoaded));
        };

        base.OnStartup(e);
    }

    private static double ToMs(long from, long to) => (to - from) * 1000.0 / Stopwatch.Frequency;

    /// <inheritdoc/>
    protected override void OnExit(ExitEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        _services?.Dispose();
        _services = null;

        base.OnExit(e);
    }
}
