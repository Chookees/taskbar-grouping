# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Fixed

- **The settings dialog no longer overwrites your settings.** Opening Settings resolved one `SettingsViewModel`, loaded it, and then resolved `SettingsWindow` - which took a second, unloaded instance through its constructor, because both were registered transient. The dialog therefore always showed constructor defaults, and clicking Save wrote those defaults over `settings.json` and switched the autostart registry entry off. Two clicks, silent data loss. The view model is now a singleton, guarded by a composition-root test.
- **`dotnet run` on the Manager can find the launcher again.** The development-layout probe derived its target framework folder from the Manager's own bin path (`net8.0-windows`), but the launcher builds into `net8.0-windows10.0.19041.0` for the WinRT taskbar projections, so the probe pointed at a path that cannot exist. `TryResolve` returned null and `GroupSyncService` aborted before writing any icon, shortcut or Start menu anchor - no group created from a dev run was pinnable. The framework folder is now enumerated instead of guessed. Installed and portable layouts were never affected, which is why CI stayed green.

### Documentation

- **The guides now describe the shipped software.** `docs/architecture.md`, `user-guide.md`, `developer-guide.md` and `api-reference.md` still carried a "Status: v0.2.0" banner and had drifted into being wrong rather than merely stale: button labels that do not exist, a pinning procedure removed three releases ago, two implemented interfaces marked "no implementation yet", and a publish command producing a layout the installer cannot consume. All four rewritten against the code; the per-file version banners are gone.
- **New:** `docs/troubleshooting.md` (symptom to log line to fix, plus the known limitations), `docs/release-process.md` (the five version-bump locations and the installer smoke checklist), `docs/adr/README.md` (index, conventions, template), `SUPPORT.md`, `THIRD-PARTY-NOTICES.md`, and an issue-chooser config.
- **New ADRs:** [ADR-002](docs/adr/002-per-group-lnk-aumid.md) records the shared-launcher-plus-AUMID design and why per-group executables were rejected; [ADR-003](docs/adr/003-dpi-unit-contract.md) records the device-pixel/DIP boundary contract. Both previously existed only as a changelog bullet and a note in the tooling file. ADR-001's two standalone-executable premises are corrected in place.
- README rewritten: the status banner is a status line again rather than a three-version changelog, Quick Start leads with the Pin to taskbar button, and the requirements section resolves the three-way minimum-OS contradiction between the installer, the launcher's target framework and the developer guide.
- `SECURITY.md` supported versions corrected from 0.1.x to 0.4.x, with a real reporting path and the trust boundaries. `CONTRIBUTING.md` aligned with how the repository is actually run. `CODE_OF_CONDUCT.md` no longer claims to be a document it is not.
- Added the Keep a Changelog link references this file had been missing since 0.1.0.

## [0.4.4] - 2026-07-09

Patch release, same day as v0.4.3 (field report minutes after install).

### Fixed

- **"+ Add" no longer looks dead.** Creating a group requires a name, but the command silently early-returned when the name box was empty — clicking "+ Add" without typing produced no reaction whatsoever, which reads as a broken button. The button is now disabled until a non-blank name is entered, the name box carries a "Group name…" watermark, and the button explains itself in a tooltip.

## [0.4.3] - 2026-07-09

Patch release. Root-causes and fixes the dead popup that v0.4.1 introduced and v0.4.2's defence-in-depth could not reach: clicking a pinned taskbar group did nothing except a brief busy cursor. Also makes launcher startup failures diagnosable from the log, fixes popup placement at non-100% display scaling, and repairs the CI/CodeQL pipeline.

### Fixed

- **Popup opens again.** Since v0.4.1 the popup window never came into existence: the open animation declared its `ScaleTransform` with `ScaleX/Y=0.5` defaults directly on the `Window`, and WPF's `Window.CoerceRenderTransform` rejects any non-identity transform — `InitializeComponent` threw during BAML load, the startup catch swallowed the exception into `Trace` (invisible in published builds), and the process exited with code 3 before anything was shown. The transform (and the scale animation targets) now live on the `ChromeRoot` content Border, which WPF scales freely. The storyboard is cloned before retargeting so a frozen resource instance cannot break `SetTarget`.
- **Launcher startup failures reach the log.** All launcher failure paths (missing group id, startup exception, unhandled AppDomain/dispatcher exceptions) previously reported via `Trace` only. A DI-free `StartupFailureLogger` now appends them to the same daily `launcher-*.log`, with a short retry against concurrent-writer sharing violations. Unhandled dispatcher exceptions shut down with documented exit code 4. This is why the v0.4.x regression stayed invisible for weeks — the failure mode is now structurally impossible.
- **Startup timing line is actually emitted.** The v0.4.1 timing summary subscribed to `Loaded` after `Show()`, but a `Window` raises `Loaded` synchronously inside `Show()` — no released build ever wrote the line. Subscription moved before `Show()`.
- **Popup placement is DPI-correct.** `TaskbarPositionHelper` fed raw device-pixel rects and the raw cursor position into WPF DIP coordinates — correct at 100% scaling only, ~33% drift at 150% (the documented v0.3 limit). All Win32 geometry is now converted to DIPs exactly once via `GetDpiForMonitor` on the target monitor, DPI awareness is switched on before the cursor capture so the anchor is genuinely physical, and the conversion is covered by placement tests at 150% including the off-screen right-edge-click repro.
- **Popup activation and dismissal are deterministic.** Popup mode now calls `Activate()` after `Show()` (parity with pin mode), `Deactivated` only dismisses after the window has genuinely held activation, and a 3 s fallback closes a never-activated popup (re-arming while the pointer is over it) so it cannot linger as an orphaned always-on-top window.
- **CodeQL runs again.** Two independent breaks: the workflow's explicit `permissions` block listed only `security-events: write`, zeroing `contents` — checkout of the private repository failed with "repository not found" on every scheduled run since 2026-05-25 — and the code-scanning upload requires GitHub Advanced Security, which is unavailable for private repos on the current plan. Fixed the permissions (+ `build-mode: manual` to match the explicit build step) and switched to `upload: never` with an in-workflow findings report: any CodeQL result now fails the job and is annotated inline, with the SARIF preserved as a 7-day artifact.
- **CI artifact uploads no longer hit the storage quota.** Coverage artifacts expire after 7 days, build output after 3, dependabot PRs skip the build-output upload entirely (two full Release bin trees per run at the 90-day default retention had exhausted the account quota), and upload steps are `continue-on-error` so a quota blip can never fail an otherwise-green build again.

### Internal

- Dependabot now updates the `Microsoft.Extensions.*` family as a single group, ignores its major bumps (net8.0 targets stay on the 8.x train; a solo 10.x bump produced NU1605 downgrade conflicts), and ignores `FluentAssertions` ≥ 8 (commercial license change).
- CLAUDE.md records the new invariants: launcher failure paths must reach the file log, `Window` accepts only identity `RenderTransform`s, `Window.Loaded` fires inside `Show()`, and the device-pixel/DIP unit contract at the Win32 boundary.

## [0.4.2] - 2026-05-18

Patch release. v0.4.1's pin + animation fixes both regressed on a real-world Win11 24H2 install: the popup stayed invisible, and the pin button showed our "Pinned" notify without any Windows system dialog and never produced a tile. v0.4.2 tightens both against timing races so they work regardless of how Win11 24H2 schedules paint and indexer work.

### Fixed

- **Popup is visible within 500 ms regardless of animation outcome.** v0.4.1 fired the storyboard from a one-shot `CompositionTarget.Rendering` subscription; Win11 24H2 can skip the composition pass entirely for a fully-transparent window, so `Rendering` never fires and the popup stays at the From state (`Opacity=0`, `Scale=0.5`) forever. v0.4.2 schedules `Storyboard.Begin` via `Dispatcher.BeginInvoke(DispatcherPriority.Render, …)` instead — the dispatcher cycle runs even when the compositor is skipped — and arms a 500 ms `DispatcherTimer` safety-net that snaps to the end state if the popup is still invisible. Also resolves `ChromeRoot` via `FindName` and `Storyboard.SetTarget` for the opacity child to bypass NameScope lookup, which can silently fail when the storyboard lives in `Window.Resources`.
- **Pin to taskbar reliably surfaces the Windows system dialog.** `TaskbarManager.RequestPinCurrentAppAsync` trusts the calling AUMID and can return success cosmetically when the Shell AppsFolder index has not yet seen the matching `.lnk`, persisting nothing. `GroupSyncService` now calls `SHChangeNotify(SHCNE_CREATE, SHCNF_PATHW | SHCNF_FLUSH, …)` after each Start Menu anchor write (on Sync and on the heal-up path), and `TaskbarPinRunner` waits 300 ms before invoking the WinRT pin API. The delay is below the ~400 ms "feels instant" threshold so users don't perceive added latency.

### Added

- **Pin-time diagnostic log.** `TaskbarPinRunner` logs the Start Menu anchor directory path, its existence flag, and the list of `*.lnk` filenames found inside it immediately before calling `RequestPinCurrentAppAsync`. Future "pin still doesn't work" reports can be triaged from `%APPDATA%/TaskbarFolders/logs/launcher-*.log` without asking the user for Explorer screenshots.

### Internal

- New `IShellChangeNotifier` / `ShellChangeNotifier` in `TaskbarFolders.Core/Shortcuts/`. P/Invoke for `shell32!SHChangeNotify` lives in the existing `Core/Interop/NativeMethods.cs` table; the wrapper interface keeps the call site testable without executing native code in CI.
- `GroupSyncService` constructor now takes `IShellChangeNotifier`. DI registration added in `ManagerServiceCollectionExtensions`. `CompositionRootTests` resolves the new service; `GroupSyncServiceTests` gains two behavioural tests covering Sync and heal-up notify-call sites.

## [0.4.1] - 2026-05-18

Patch release. Three v0.4.0 regressions reported from real-world install on Win11 24H2:

### Fixed

- **Pin to taskbar button actually pins now.** v0.4.0 wrote the per-group `.lnk` only under `%APPDATA%/TaskbarFolders/shortcuts/<id>.lnk` — invisible to the Shell AppsFolder index. `TaskbarManager.RequestPinCurrentAppAsync` requires a Start Menu shortcut with the matching AUMID to anchor the pin; without one the call silently fails to persist even though the system dialog shows and the API returns success. v0.4.1 writes a sibling `.lnk` at `%APPDATA%/Microsoft/Windows/Start Menu/Programs/TaskbarFolders/<display-name>.lnk` on every sync. A heal-up loop runs on Manager startup so existing v0.4.0 groups also gain Start Menu anchors.
- **Open animation visible on cold launches.** v0.4.0 fired the storyboard from `OnSourceInitialized`, but WPF first paint happened 100-200 ms later — by the time the user saw frame 1, the 200 ms timeline had elapsed and the popup snapped to its end state. v0.4.1 sets XAML defaults to match the animation's From state (Border `Opacity=0`, `ScaleX/Y=0.5`) and uses a one-shot `CompositionTarget.Rendering` subscription to start `Storyboard.Begin` on the first composition frame. Also avoids the Win11 24H2 paint-skip path triggered by `AllowsTransparency=True + Window.Opacity=0` by animating the inner Border's opacity instead of the Window's.

### Added

- **Per-checkpoint startup timing log.** Launcher `popup-mode` now logs a single line on first paint with the wall-clock ms from `tStart` to each phase (`aumid`, `settings`, `di`, `theme`, `vm`, `show`, `loaded`) plus `processAge` capturing the .NET runtime cold-start cost that happens before our code runs. Read via `%APPDATA%/TaskbarFolders/logs/launcher-*.log`. Will inform the v0.4.2 / v0.5 perf work — until we know where the user's reported ~1 s per click goes, architectural changes (persistent launcher daemon, drop `PublishSingleFile`, etc.) would be speculative.

### Known limitations (deferred)

- **Group rename leaves the old Start Menu .lnk as an orphan.** The new filename gets written but the old one remains. v0.5 will add a reconciler that enumerates `StartMenuDirectory` and deletes anchors whose AUMID does not match any current group.

## [0.4.0] - 2026-05-18

Minor release. Launcher polish triggered by hands-on use of v0.3.0: the open animation felt too subtle, the popup still had measurable startup overhead, and pinning a group to the taskbar took three manual clicks through the Explorer context menu.

### Changed

- **Popup grows up out of the clicked tile.** The open storyboard now scales 0.5→1.0 over 200 ms with a `QuinticEase` curve, and the `ScaleTransform` pivot moved from top-left to the bottom-centre of the popup (`CenterX = Width/2`, `CenterY = Height`). Since the popup sits directly above the clicked tile per `TaskbarPositionHelper`, the pivot-at-bottom-centre produces a "grow up out of the tile" feel instead of a fade-in-place. The animation now fires from `OnSourceInitialized` (before first paint) so frame 1 already shows scale=0.5 rather than a brief snap-to-1.0.
- **One-click pin to taskbar.** The new **Pin to taskbar** button in the Manager spawns the Launcher in `--pin-mode`, which calls `Windows.UI.Shell.TaskbarManager.RequestPinCurrentAppAsync()`. Windows shows its native "Allow [App] to pin?" dialog; clicking Allow pins the group with its distinct AUMID. The previous **Show shortcut...** behaviour remains as a secondary button for users on Windows builds where TaskbarManager is unsupported (LTSC, Education) — the Manager auto-falls-back to the Explorer flow in that case.

### Performance

- **Deferred startup IO.** `FileSystemIconCache.PruneStaleEntries` and `FileLoggerProvider.PruneOldFiles` moved out of their constructors into `StartBackgroundPrune()` methods that App.OnStartup fires post-`Show()`. Saves ~15-70 ms on the first-paint critical path depending on cache + log retention state.
- **Explicit popup dimensions.** `PopupWindow.SizeToContent="WidthAndHeight"` replaced with an explicit `Width = cols * 96 + 24, Height = rows * 96 + 24` computation in `OnSourceInitialized`. Skips the WPF measure pass that used to add ~5-10 ms before placement could be computed.
- **DI graph validation only in Debug.** `ServiceProvider` is built with `ValidateOnBuild=true / ValidateScopes=true` in Debug builds only. Release skips the eager-walk (~20-30 ms). `CompositionRootTests` exercise the validation explicitly so the safety net is preserved in CI.

### Internal

- `Windows.UI.Shell.TaskbarManager` requires WinRT projections, so `TaskbarFolders.Launcher.csproj` TFM bumped from `net8.0-windows` to `net8.0-windows10.0.19041.0` (Win10 1903+). Launcher.Tests TFM matches; Manager + Shared stay on `net8.0-windows`.
- `TaskbarPinRunner` attaches the WinRT manager to a foreground HWND via `WinRT.Interop.InitializeWithWindow.Initialize` so the system pin dialog parents correctly on multi-monitor / multi-foreground setups.
- `PinHostWindow` (1×1, fully transparent, centred on the primary screen) is the foreground HWND the WinRT pin dialog attaches to. Off-screen positioning broke foreground promotion on Win11 24H2 — centred + `Opacity=0` is the working compromise.
- `GroupAumid.TryExtractGroupId` reverses `GroupAumid.For` so the Launcher can recover its group id from the AUMID Windows assigned to the process. Used when Windows launches a TaskbarManager-pinned tile without preserving the original `--group-id` command line. Case-insensitive prefix match mirrors Windows' own wcsicmp-based AUMID comparison.
- `App.OnStartup` split into `RunPinModeAsync` + `RunPopupModeAsync` branches with a shared `ResolveGroupId` helper. Both branches wrapped in try/catch so any unhandled async-void exception produces a documented exit code (`Shutdown(3)`) instead of a random WPF crash code.
- `LauncherProcessPinService` (Manager) spawns the Launcher via the new `IProcessRunner` abstraction (testable Process.Start wrapper) with a 2-minute timeout, maps the exit code (0=Success, 1=UserDenied, 2=Unsupported, 3=Error) to a `PinResult` enum. View model receives the result and routes Notify accordingly.
- `IIconCache.StartBackgroundPrune` is a C# 8 default interface method (no-op default) so existing mocks continue to work.

## [0.3.0] - 2026-05-17

Minor release. Launcher popup polish triggered by hands-on use of v0.2.1: the popup opened slowly, the chrome was too visible, and placement was anchored on the taskbar centre rather than the clicked tile.

### Changed

- **Popup opens instantly.** `PopupViewModel.LoadAsync` is now metadata-only — reads the group config and populates the tile collection with empty icons in ~5 ms. The window paints immediately; a new `StartIconLoad` fires per-app icon extraction on the thread pool and assigns each `Icon` as it resolves. Pre-v0.3 the launcher froze the UI thread 200 ms-3 s on cold cache before the first paint.
- **Popup chrome is fully transparent.** The semi-opaque Border, the rounded corners, the drop shadow, and the Win11 Acrylic backdrop are all gone. Only the icons and the per-tile hover highlight are visible — the popup feels like floating icons rather than a card. Error states (missing group, launch failure) get their own per-element backdrop so the text stays readable on any wallpaper.
- **Popup is anchored on the clicked tile.** `App.OnStartup` now captures `GetCursorPos` as its very first instruction (before WPF bootstrap can let the cursor drift) and seeds the new `ICursorAnchor` singleton. `TaskbarPositionHelper.CalculatePlacement` centres the popup horizontally on the cursor X (top/bottom taskbar) or vertically on the cursor Y (side taskbar), still clamped to the monitor work area. Pre-v0.3 placement used the taskbar geometric centre regardless of tile position, which felt "random" for any tile not in the middle.

### Performance

- Settings JSON is now loaded exactly once per launcher startup (v0.2 loaded it twice — once in `App.OnStartup` for theme, once again inside `PopupWindow.PositionAndConfigureAsync`).
- `PublishReadyToRun=true` enabled for both Launcher and Manager. Ahead-of-time native compile saves ~100-200 ms of first-launch tiered-JIT warm-up; ZIP grows ~10-20 MB total which is acceptable for the perf gain on the per-click launcher critical path.

### Known limitations

- **DPI scaling** — `GetCursorPos` returns physical pixels in system-DPI space while WPF positions in DIPs. On 100% scaling these match; on 150%+ scaling the popup may be horizontally off by up to ~33% of the popup width. v0.2 already had the same bug for the taskbar rect, so v0.3 does not regress baseline behaviour. Per-monitor DPI scaling is planned for v0.3.1.

### Internal

- New `ICursorAnchor` / `LauncherCursorAnchor` (singleton, throws if `Position` read before `Seed`, last-write-wins on double-Seed).
- `TaskbarPositionHelper.CalculatePlacement` static signature gained a `Point clickAnchor` parameter — binary-incompatible but no external consumers.
- `PopupWindow` constructor now takes `AppSettings` instead of `IAppSettingsStore`.
- `PopupViewModel` implements `IDisposable` (CA1001) — disposes the icon-load `CancellationTokenSource`.
- 14 new tests: 4 `PopupViewModelTests` for the split (LoadAsync no-extractor contract, StartIconLoad parallel extraction, cache-hit fast path, cancellation), 6 `TaskbarPositionHelperAnchorTests` covering cursor-anchored placement + edge clamping, 3 `CursorAnchorTests` for the contract, 1 added `CompositionRootTests` registration check for `ICursorAnchor`. Existing `TaskbarPositionHelperTests` (10 cases) reworked to thread the new `clickAnchor` parameter through.

## [0.2.1] - 2026-05-17

Patch release. The "Show shortcut..." button was a silent no-op for every installer and portable-ZIP user of v0.2.0.

### Fixed

- **Show shortcut button now works in installed builds.** `LauncherPathResolver` only checked for `TaskbarFolders.Launcher.exe` as a side-by-side neighbour of `TaskbarFolders.Manager.exe`. The Inno Setup installer (and the release `Compress-Archive` of `./publish/*`) places them in sibling folders (`{app}\Manager\` and `{app}\Launcher\`), so resolution returned null on every shipped build, `GroupSyncService` silently skipped shortcut generation, and the pin-helper command did nothing when clicked. Added a sibling-folder probe between the existing side-by-side and dev-layout strategies.
- **Pin-helper now surfaces missing-shortcut conditions.** When the `.lnk` does not exist, the command runs a one-shot `SyncAsync` (covers the case where an earlier sync was skipped due to a now-fixed environment), then opens a dialog naming the log location instead of returning silently. Exceptions from the inline sync are caught and routed through the same dialog so they cannot escape as unhandled `AsyncRelayCommand` failures.
- `GroupSyncService` log level for unresolved launcher bumped from Warning to Error — it is a user-blocking condition, not a soft warning. `LauncherPathResolver` itself now logs the full probed-paths list so support logs pinpoint which assumption broke.

### Internal

- `IUserConfirmation` gained `Notify(caption, message)` for one-button information dialogs (backed by `MessageBox` with `OK` + `Information` icon).
- `LauncherPathResolver` exposes `internal TryResolveFrom(string baseDirectory)` so the probe sequence is exercised against fixture directories rather than `AppContext.BaseDirectory`. `InternalsVisibleTo TaskbarFolders.Manager.Tests` added to the Manager csproj.
- 5 new resolver tests (installer-layout regression, side-by-side preference, no-match contract, blank-arg rejection, contract from v0.2.0) and 4 new view-model tests for the pin-helper happy path, sync-cannot-recover path, no-binding no-op, and SyncAsync-throws path.

## [0.2.0] - 2026-05-17

First functional release. Everything described in the README is implemented and tested.

### Added

#### Manager
- Sidebar group list with **+ Add**, alphabetical sort, right-click **Delete group** (with unpin warning), inline rename via the group editor
- Group editor: drag & drop `.exe`/`.lnk` from Explorer, **Add app...** file picker, per-app **Remove**, live 256×256 composite-icon preview with 300 ms debounce
- Settings dialog with theme (System/Light/Dark), popup position (Auto/Above/Below), animations toggle, **Start with Windows** (per-user HKCU\Run entry — no elevation)
- Pin-to-taskbar helper that opens Explorer with the group's `.lnk` pre-selected
- Mica backdrop on Windows 11 22H2+; themed solid background on older Windows
- Live theme switching when Windows app theme changes (via `SystemEvents.UserPreferenceChanged`)

#### Launcher
- Acrylic popup on Windows 11 22H2+ with rounded corners + drop shadow
- Configurable grid columns (1–6) bound per group; tile hover highlight; click launches via `Process.Start` with `UseShellExecute`
- 150 ms fade + scale open animation (respects the global animations toggle)
- Click-outside-to-close (`Deactivated` event)
- Inline error banner when a launch fails (popup stays open for retry)
- Multi-monitor placement via `SHAppBarMessage`/`MonitorFromPoint`/`GetMonitorInfo`; handles bottom/top/left/right taskbars and secondary monitors with negative X
- Per-monitor V2 DPI awareness via `SetProcessDpiAwarenessContext`

#### Pinning architecture
- `IShortcutGenerator` writes a `.lnk` per group via `IShellLinkW` + `IPersistFile` + `IPropertyStore` (atomic `.tmp` + `File.Move`); stamps `PKEY_AppUserModel_ID` so each pinned tile has a distinct identity even though they all target the single signed `Launcher.exe`
- Launcher calls `SetCurrentProcessExplicitAppUserModelID` early in startup so the running process joins its pinned tile
- `GroupAumid` helper is the single source of truth for the AUMID format; both sides consume it
- Per-group artifact sync: every save regenerates the composite `.ico` and refreshes the `.lnk`

#### Core / Shared infrastructure
- `IGroupConfigStore` + `JsonGroupConfigStore` for per-group JSON persistence in `%APPDATA%/TaskbarFolders/groups/`; atomic writes via `.tmp` + `File.Move`
- `IAppSettingsStore` + `JsonAppSettingsStore` for global settings
- `IAppDataPathProvider` centralises every `%APPDATA%` path; group ids validated against `^[A-Za-z0-9._-]{1,96}$` so a hand-edited config cannot escape the per-app data root
- `ThemePreference` and `PopupPositionPreference` strongly-typed enums replace the original string preferences; values serialised in camelCase via `CamelCaseEnumConverter<T>`; `GroupConfig.Columns` clamps to `[1..6]` on assignment
- `IIconExtractor` (`ShellIconExtractor`) extracts icons from `.exe`/`.lnk`/`.ico` via `SHGetFileInfo` + `IImageList`; resolves `.lnk` targets via `IShellLinkW`
- `ICompositeIconGenerator` (`CompositeIconGenerator`) produces 1/2/3/4-tile iOS-style composites
- `IIcoFileWriter` (`IcoFileWriter`) writes multi-resolution PNG-in-ICO files (16/32/48/256)
- `IIconCache` (`FileSystemIconCache`) caches extracted icons by source-path + last-write-time hash; prunes entries older than 30 days
- `Microsoft.Extensions.Logging` rotating file sink under `%APPDATA%/TaskbarFolders/logs/`; retention default 14 days
- `System.Text.Json` source-generator context for trim/AOT-readiness
- `WindowBackdrop` helper wraps `DwmSetWindowAttribute(DWMWA_SYSTEMBACKDROP_TYPE)` for both Mica and Acrylic
- `app.manifest` for Manager declaring `PerMonitorV2` DPI awareness

#### Testing & CI
- ~170 unit tests across `TaskbarFolders.Core.Tests`, `TaskbarFolders.Manager.Tests`, `TaskbarFolders.Launcher.Tests`
- DI composition tests for both Manager and Launcher (`ValidateOnBuild` catches lifetime mismatches at provider construction)
- Multi-monitor edge tests covering negative-X secondary monitors, oversized popups, exact-fit boundaries, all preference values
- CI emits HTML coverage report via ReportGenerator + console summary in the workflow log

### Changed
- Strategy for per-group pinning chosen during the M5 spike: `.lnk` shortcuts with distinct AUMIDs targeting a single host `Launcher.exe`, rather than the originally-planned per-group native `.exe` via `BeginUpdateResource`. Eliminates the AV/Defender false-positive risk for unsigned dynamically-modified PEs in `%APPDATA%`.

### Fixed
- Release-workflow version extraction now uses PowerShell-native syntax instead of Bash parameter expansion (the workflow runs on `windows-latest` with `pwsh` as default).

## [0.1.0] - 2026-05-06

### Added

- Initial project structure with solution and all sub-projects
- Core library with icon extraction and composite icon generation interfaces
- Shared library with models (AppEntry, GroupConfig, AppSettings)
- Manager application scaffold (WPF, MVVM)
- Launcher application scaffold (WPF popup window)
- CI pipeline with build, test, and format checking
- Release pipeline with self-contained publish and Inno Setup installer
- CodeQL security analysis
- Dependabot configuration for NuGet and GitHub Actions
- Full documentation: README, Contributing Guide, Architecture, User Guide, Developer Guide
- MIT License

[Unreleased]: https://github.com/eXORR6077/taskbar-grouping/compare/v0.4.4...HEAD
[0.4.4]: https://github.com/eXORR6077/taskbar-grouping/compare/v0.4.3...v0.4.4
[0.4.3]: https://github.com/eXORR6077/taskbar-grouping/compare/v0.4.2...v0.4.3
[0.4.2]: https://github.com/eXORR6077/taskbar-grouping/compare/v0.4.1...v0.4.2
[0.4.1]: https://github.com/eXORR6077/taskbar-grouping/compare/v0.4.0...v0.4.1
[0.4.0]: https://github.com/eXORR6077/taskbar-grouping/compare/v0.3.0...v0.4.0
[0.3.0]: https://github.com/eXORR6077/taskbar-grouping/compare/v0.2.1...v0.3.0
[0.2.1]: https://github.com/eXORR6077/taskbar-grouping/compare/v0.2.0...v0.2.1
[0.2.0]: https://github.com/eXORR6077/taskbar-grouping/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/eXORR6077/taskbar-grouping/releases/tag/v0.1.0
