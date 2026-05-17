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
- Per-group composite icon: `%APPDATA%/TaskbarFolders/icons/<id>.ico`
- Per-group shortcut (Strategy C `.lnk`): `%APPDATA%/TaskbarFolders/shortcuts/<id>.lnk`
- Settings: `%APPDATA%/TaskbarFolders/settings.json`
- Icon cache: `%APPDATA%/TaskbarFolders/icons/cache/<sha256>.png`
- Logs: `%APPDATA%/TaskbarFolders/logs/{manager,launcher}-yyyy-MM-dd.log`

## Installed layout (read before touching `LauncherPathResolver`)

The Inno Setup installer and the portable ZIP both ship Manager and Launcher in **sibling folders**, not one directory:

```
{app}\Manager\TaskbarFolders.Manager.exe
{app}\Launcher\TaskbarFolders.Launcher.exe
```

`LauncherPathResolver` probes three layouts (`side-by-side` → `sibling folder` → `dev sln walk-up`). Any new packaging must match one of those, or the resolver gains a fourth probe. The v0.2.0 release shipped without the sibling probe and the `Show shortcut...` button was silently broken for every install — regression test in `LauncherPathResolverTests.TryResolveFrom_FindsLauncherInSiblingFolder_MatchingInstallerLayout` guards against repeating it.

## Workflow for non-trivial work

This project is solo-maintained; senior-dev workflow expectations apply.

**1. Analyse before changing anything.** Read the files end-to-end (XAML binding → VM command → service → P/Invoke), check the runtime log under `%APPDATA%/TaskbarFolders/logs/`, and form a falsifiable root-cause hypothesis. Quote line numbers when you state the cause.

**2. Orchestrate subagents when the task warrants it.** Rule of thumb: trivial edits go direct; multi-file fixes get a `Plan` agent first; bug investigations across more than two files get an `Explore` agent in parallel; anything user-blocking gets a `general-purpose` agent for independent code review **before push**. Brief each agent self-contained (file paths, line numbers, constraints from this CLAUDE.md) — they cannot see the conversation.

**3. Bug-fixing waves.** Group changes into reviewable commits: one commit per behavioural concern (e.g. resolver fix, UX fix, UX defence-in-depth), each with its own tests. Run `dotnet build -c Release` after every wave (tests need WindowsDesktop 8.0 x64 which is CI-only). Address code-review findings as a polish commit on top — never amend a pushed commit, never amend across reviewable boundaries.

**4. Complete testing — including the runtime layout that ships.** Build + unit tests in CI is necessary but not sufficient. The v0.2.0 "Show shortcut" bug existed *only* in the installed and portable layouts; the dev `dotnet run` and the CI test runner both used a layout where the launcher happened to be findable. Every release-eligible change must be verified in all three runtimes it touches:

  - **Unit tests** — `dotnet test -c Release` (CI) — must be green before push, no exceptions. Pushing red and "fixing forward" is forbidden because it muddies bisects.
  - **Format gate** — `dotnet format --verify-no-changes` (CI) — fix locally before push.
  - **Dev run** — `dotnet run --project src/TaskbarFolders.Manager` — for any change that touches MVVM, DI, XAML, or services. Click the affected button.
  - **Installer/portable smoke** — after tagging, before announcing: actually install `TaskbarFolders-Setup.exe` (or unzip the portable) and walk the user-visible happy path end-to-end (create group → drop apps → click the affected feature). Two minutes of clicking catches what 200 unit tests do not. **This step would have caught the v0.2.0 bug before users did.**
  - **Log inspection** — open `%APPDATA%/TaskbarFolders/logs/manager-*.log` after the smoke test. A warning or error line with no user-visible counterpart is a UX bug.

  If you cannot run the installer smoke (e.g. no Windows machine to hand), say so explicitly in the PR/commit message rather than imply the feature is verified. Future-you reading the log will know what was actually tested.

**5. Commits.** Conventional Commits (`fix(manager): …`, `feat(core): …`). Body explains the *why* and the *blast radius*, not the *what* — assume the reader has the diff. No AI/agent mentions, no `Co-Authored-By` trailers. Run `dotnet format` before staging — the CI `--verify-no-changes` step is unforgiving about charset (UTF-8 BOM) and line endings (CRLF for `.cs`/`.xaml`).

**6. Pushing & releasing.** Bug fixes land on `develop` directly (no PR — solo-maintained). Patch releases tag `v0.x.y` from `develop`; `release.yml` publishes the assets. Bump `Directory.Build.props:Version`, `installer/setup.iss:MyAppVersion`, the README status banner, and add a `CHANGELOG.md` section in the same commit as the tag-eligible state. If a fix is user-blocking on a shipped version, cut a patch release the same day. **Wait for the installer-smoke pass before announcing the release as available** — the tag and the assets exist before they're verified.

**7. CLAUDE.md upkeep.** When a fix introduces a new convention or invariant, surface it here — the next session reads this file before doing anything else.

## Release

Pushing a `v*` tag triggers `release.yml`: builds, publishes self-contained, builds the Inno Setup installer (ISCC is pre-installed on `windows-latest`), uploads `TaskbarFolders-portable.zip` + `TaskbarFolders-Setup.exe`. The release job needs `permissions: contents: write` (already set).
