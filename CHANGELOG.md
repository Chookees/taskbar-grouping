# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
