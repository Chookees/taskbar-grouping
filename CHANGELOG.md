# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
