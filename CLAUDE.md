# TaskbarFolders

Windows-only WPF/.NET 8 desktop app. iOS-style taskbar grouping for Windows 11.

## Commands

```bash
dotnet build TaskbarFolders.sln -c Release
dotnet test  TaskbarFolders.sln -c Release
dotnet format --verify-no-changes      # CI enforces — run before commit
```

**Local test requirement:** `Microsoft.WindowsDesktop.App 8.0.x x64` runtime must be installed. Linux/macOS cannot run the tests; CI (`windows-latest`) can. SDK is pinned in `global.json` (8.0.100, `rollForward=latestMajor`).

## Architecture

Four-project solution. Dependencies flow Manager/Launcher → Core → Shared.

| Project | Purpose | Constraint |
|---|---|---|
| `TaskbarFolders.Shared` | Models, JSON persistence, file logging | **No Windows-only APIs** — keep portable |
| `TaskbarFolders.Core` | Icon engine, Win32 P/Invoke, shortcut generation | `[SupportedOSPlatform("windows")]` |
| `TaskbarFolders.Manager` | WPF group-CRUD app | DI via `ManagerServiceCollectionExtensions` |
| `TaskbarFolders.Launcher` | Per-group popup (short-lived per click) | DI via `LauncherServiceCollectionExtensions` |

## Project Conventions

- **Strict MVVM** — no business logic in code-behind. Commands via `[RelayCommand]` (CommunityToolkit.Mvvm 8.3.2).
- **DI everywhere** — register in `*ServiceCollectionExtensions`, inject via constructor. `CompositionRootTests` validates the Manager graph at build.
- **File-scoped namespaces**, `_camelCase` private fields, PascalCase for `const` fields.
- **XML doc comments required** on public members (analyzer-enforced).
- **Async I/O** — all persistence and shell calls async, except `IIconCache.TryGet/Set` (sync on purpose; UI hot path).

## Non-Obvious Patterns

**Atomic writes** — used in `JsonGroupConfigStore`, `JsonAppSettingsStore`, `IcoFileWriter`, `FileSystemIconCache`:

```csharp
File.WriteAllBytes(target + ".tmp", bytes);
File.Move(target + ".tmp", target, overwrite: true);
```

New persistence code must follow this.

**HICON discipline** — every `SHGetFileInfo` / `IImageList.GetIcon` `HICON` must be released in `finally` via `DestroyIcon`; every COM RCW via `Marshal.FinalReleaseComObject`. Instantiate the RCW **before** `try` so a CLSID-not-registered failure doesn't leave the `finally` with a half-constructed object (see `ShellIconExtractor.ResolvePath`).

**Pin-to-taskbar = Strategy C** — per-group `.lnk` with a distinct AUMID stamped via `IPropertyStore` + `PKEY_AppUserModel_ID`. The `Launcher.exe` is shared; grouping is purely AUMID-driven. **Do not** introduce per-group `.exe` copies + `UpdateResource` (Strategy A) — ruled out in M5 spike because unsigned dynamically-modified PEs trigger Defender false positives.

**GroupId validation** — `AppDataPathProvider` enforces `^[A-Za-z0-9._-]{1,96}$`. Any path-derived storage code (groups, per-group shortcut dirs) must funnel through `GetGroupFile` / `GetGroupDirectory` so the validation isn't bypassed.

**JSON config** — `Id` in `GroupConfig` is always reconstructed from the file name on load (disk layout is the source of truth; JSON `id` field is ignored). See `JsonGroupConfigStore.LoadFromFileAsync`.

## Analyzer Suppressions (rationale)

Project-wide `EnforceCodeStyleInBuild=true` + `TreatWarningsAsErrors=true`. Suppressions in `.editorconfig`:

- `CA1716` global — `TaskbarFolders.Shared` namespace clashes with a VB.NET reserved word (irrelevant for Windows-only C# app).
- `CA1848` / `CA1873` global — `LoggerMessage` source generators not used; cold-path logging only. Hot paths can opt in.
- `CA1707` / `CA1859` / `CA1861` `tests/**` only — xUnit naming convention + perf rules don't apply to test code.
- `IDE1006` const-field rule — separate `required_modifiers=const` naming rule allows `PascalCase` const fields alongside `_camelCase` private fields.

If you suppress an analyzer, add the rationale in `.editorconfig` next to the suppression.

## Repo Policy

- **No AI/agent mentions** in code, comments, commits, docs, or any human-facing committed file. Tooling files like this one are exempt.
- **Conventional Commits** — `<type>(<scope>): <desc>`. Types per `CONTRIBUTING.md`.
- **Branching** — `develop` is integration, `main` is releases, feature branches off `develop`.

## Where things live at runtime

- Group configs: `%APPDATA%/TaskbarFolders/groups/<id>.json`
- Per-group shortcut (Strategy C `.lnk`): `%APPDATA%/TaskbarFolders/groups/<id>/<name>.lnk`
- Settings: `%APPDATA%/TaskbarFolders/settings.json`
- Icon cache: `%APPDATA%/TaskbarFolders/icons/cache/<sha256>.png`
- Logs: `%APPDATA%/TaskbarFolders/logs/{manager,launcher}-yyyy-MM-dd.log`

## Release

Pushing a `v*` tag triggers `release.yml`: builds, publishes self-contained, builds the Inno Setup installer (ISCC is pre-installed on `windows-latest`), uploads `TaskbarFolders-portable.zip` + `TaskbarFolders-Setup.exe`. The release job needs `permissions: contents: write` (already set).
