using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Extensions.Logging;
using Windows.Foundation.Metadata;
using Windows.UI.Shell;
using WinRT.Interop;

namespace TaskbarFolders.Launcher.Services;

/// <summary>
/// Wraps <see cref="TaskbarManager.RequestPinCurrentAppAsync"/> with exit-code semantics so
/// the Manager-spawned launcher process can communicate the outcome back without IPC.
/// </summary>
/// <remarks>
/// Exit codes:
/// <list type="bullet">
///   <item><c>0</c> — pinned (user clicked Allow in the system dialog).</item>
///   <item><c>1</c> — user denied (clicked Cancel or closed the dialog).</item>
///   <item><c>2</c> — TaskbarManager unsupported on this Windows build or pinning policy
///   disabled (LTSC, Education, restricted Group Policy). Manager falls back to the
///   Explorer / .lnk flow.</item>
///   <item><c>3</c> — unexpected exception during the call. Logged.</item>
/// </list>
/// </remarks>
[SupportedOSPlatform("windows10.0.19041.0")]
public sealed class TaskbarPinRunner
{
    private readonly ILogger<TaskbarPinRunner>? _logger;

    /// <summary>Initializes a new instance.</summary>
    public TaskbarPinRunner(ILogger<TaskbarPinRunner>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Asks Windows to pin the current process (identified by its AUMID) to the taskbar.
    /// Must be called from the UI thread; <paramref name="foregroundWindow"/> must be
    /// visible + activated so the system dialog has a parent HWND.
    /// </summary>
    public async Task<int> RunAsync(Window foregroundWindow)
    {
        ArgumentNullException.ThrowIfNull(foregroundWindow);

        try
        {
            // ApiInformation.IsTypePresent is the canonical "is this WinRT API surface
            // available?" check for desktop-bridge / Win32-with-WinRT callers. Falsy on
            // pre-Win10-1903 builds; returns true on Win11 24H2 + LTSC + Education.
            if (!ApiInformation.IsTypePresent("Windows.UI.Shell.TaskbarManager"))
            {
                _logger?.LogWarning("Windows.UI.Shell.TaskbarManager not present; pin unavailable.");
                return 2;
            }

            var manager = TaskbarManager.GetDefault();
            if (manager is null || !manager.IsPinningAllowed)
            {
                _logger?.LogWarning("TaskbarManager.IsPinningAllowed=false (likely restricted SKU or policy); pin unavailable.");
                return 2;
            }

            // CRITICAL: Win32 desktop callers MUST attach the WinRT instance to a HWND via
            // InitializeWithWindow.Initialize before invoking any modal-UI method. Without it
            // the system "Allow [App] to pin?" dialog has no parent on multi-monitor /
            // multi-app foregrounds and either appears behind other windows or fails silently.
            var hwnd = new WindowInteropHelper(foregroundWindow).EnsureHandle();
            InitializeWithWindow.Initialize(manager, hwnd);

            // v0.4.2 diagnostic: log the Start Menu anchor directory contents at the exact
            // moment of the pin call. RequestPinCurrentAppAsync can return true cosmetically
            // when the Shell's AppsFolder index hasn't materialised the .lnk yet; this log
            // line lets us tell "anchor missing" from "anchor present but indexer raced".
            // Enumeration is defensive: a Defender lock / ACL change must not escalate a
            // diagnostic failure into a failed pin (the outer catch would map it to exit 3).
            var anchorDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Programs),
                "TaskbarFolders");
            var anchorExists = Directory.Exists(anchorDir);
            string anchorFiles;
            try
            {
                anchorFiles = anchorExists
                    ? string.Join(", ", Directory.EnumerateFiles(anchorDir, "*.lnk").Select(Path.GetFileName))
                    : "<dir missing>";
            }
            catch (Exception enumEx) when (enumEx is IOException or UnauthorizedAccessException or System.Security.SecurityException)
            {
                anchorFiles = $"<enum failed: {enumEx.GetType().Name}>";
            }
            _logger?.LogInformation(
                "Pin runner: Start Menu anchor dir={Dir} exists={Exists} contents=[{Files}]",
                anchorDir, anchorExists, anchorFiles);

            // v0.4.2 settle: GroupSyncService already SHChangeNotify-flushes, but the
            // AppsFolder index can still take a moment to surface a brand-new entry. 300 ms
            // is below the ~400 ms "feels instant" threshold so the user does not perceive
            // it as latency between clicking Pin and seeing the system dialog.
            await Task.Delay(300).ConfigureAwait(true);

            // RequestPinCurrentAppAsync shows a system-managed "Allow [App] to pin?" dialog
            // parented to the HWND we just attached above.
            var pinned = await manager.RequestPinCurrentAppAsync();

            if (pinned)
            {
                _logger?.LogInformation("TaskbarManager pinned the current app.");
                return 0;
            }

            _logger?.LogInformation("TaskbarManager: user declined the pin request.");
            return 1;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "TaskbarManager pin call threw unexpectedly.");
            return 3;
        }
    }
}
