# Contributing to TaskbarFolders

Thanks for your interest. This document covers how work gets into the repository. For building, debugging and the conventions the compiler enforces, see the [Developer Guide](docs/developer-guide.md).

## Development setup

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) — `global.json` pins 8.0.100 and rolls forward, so a newer SDK is fine
- Windows. The solution targets `net8.0-windows*` and neither builds nor tests elsewhere
- Visual Studio 2022, JetBrains Rider, or VS Code with the C# Dev Kit

```bash
git clone https://github.com/eXORR6077/taskbar-grouping.git
cd taskbar-grouping
dotnet build TaskbarFolders.sln -c Release
dotnet test TaskbarFolders.sln -c Release
```

The clone directory is `taskbar-grouping`; the product is called TaskbarFolders.

If your machine has a newer WindowsDesktop runtime than 8.0.x, prefix test and run commands with `DOTNET_ROLL_FORWARD=LatestMajor`.

## Branching

| Branch | Purpose |
|---|---|
| `main` | Released state. Every release tag is contained here |
| `develop` | Integration branch and the repository default |
| `feature/<name>` | New functionality |
| `fix/<name>` | Bug fixes |
| `docs/<name>` | Documentation-only changes |

Branch from `develop` and target `develop`. `main` moves forward only at release time.

CI runs on pushes to `develop`, `feature/*` and `fix/*`, and on every pull request into `develop`. A branch named outside those patterns still gets a full CI run through its pull request.

The project is solo-maintained: the maintainer lands small fixes directly on `develop`. Contributions from anyone else go through a pull request, and anything user-facing or non-trivial should go through one regardless of who wrote it.

## Before you push

All three must pass locally. CI enforces the same, and the format check is a hard gate.

```bash
dotnet format TaskbarFolders.sln
```

```bash
dotnet build TaskbarFolders.sln -c Release
```

```bash
dotnet test TaskbarFolders.sln -c Release
```

Run the formatter **before staging each commit**, not once at the end of a series. A release CI run has failed because the formatter fixed files that had already been committed earlier in the branch and the fix stayed in the working tree, unstaged. The sequence that works is: format, check `git status`, stage everything modified, commit.

If your change touches the UI, view models, dependency injection or services, also run the application and click the thing you changed. The test suite is headless and does not open a window.

## Commits

[Conventional Commits](https://www.conventionalcommits.org/), format `<type>(<scope>): <description>`.

| Type | Use for |
|---|---|
| `feat` | New functionality |
| `fix` | Bug fix |
| `docs` | Documentation |
| `style` | Formatting only, no behaviour change |
| `refactor` | Restructuring with no behaviour change |
| `test` | Tests |
| `ci` | Pipeline changes |
| `build` | Build system changes |
| `perf` | Performance |
| `chore` | Maintenance |

Scope is the project or area: `manager`, `launcher`, `core`, `shared`, `deps`, `release`.

```
feat(core): add 2x2 composite icon generation
fix(launcher): resolve popup positioning on multi-monitor setups
docs(readme): document the pin-to-taskbar flow
test(core): add unit tests for the icon cache
```

Write the body for someone who already has the diff. Explain **why** the change is right and what its blast radius is — what else it touches, what it does not, and why it is safe. Do not restate what the diff shows.

Group related work into one commit per behavioural concern rather than one commit per file, so a later `git bisect` lands on something meaningful.

## Pull requests

1. Fill in the template.
2. Make sure CI is green — build, tests and the format check.
3. Note what you verified manually. "Unit tests pass" and "I used the feature" are different claims; if you could not run something, say so rather than leaving it implied.
4. Update the documentation your change affects — see below. This is part of the change, not a follow-up.

## Definition of Done

A change is finished when all of this holds, not when the code works:

| | |
|---|---|
| Build, tests, format | `dotnet build -c Release`, `dotnet test -c Release`, `dotnet format TaskbarFolders.sln --verify-no-changes`. Read the format output rather than trusting the exit code, and give new `.cs` files CRLF line endings and a UTF-8 BOM |
| Coverage | CI fails below 65 % line coverage, and at exactly 0 % — an empty report reads like an untested code base |
| Documentation | Moves with the code, in the same commit. See the table below |
| Screenshots | Retaken when the UI changes visibly, per the recipe in `CLAUDE.md` so they stay comparable |
| Visual changes | Looked at, not asserted — screenshot the window or render the control offscreen |
| Interop / WinRT | Actually executed. The suite is headless and cannot reach WinRT; pin-to-taskbar was broken for eight releases behind a cast no test could see |
| New gates | Fed an input that must fail, to prove the gate fires |
| Limitations | Opened as issues with file references and acceptance criteria, not left in prose |
| `CHANGELOG.md` | An entry under `[Unreleased]`. Release notes are built from these sections |

## Documentation moves with the code

**Every change, implementation or update also brings the affected documentation up to date, in the same commit.** A behaviour change whose documentation still describes the old behaviour is not finished, and will not be merged as-is.

This is not ceremony. The guides once sat three minor versions behind and actively misdescribed how pinning worked — telling users to pin an executable, an approach the project had already abandoned. Someone following them could not complete a single task.

| If you changed | Also update |
|---|---|
| User-visible behaviour, a button, a dialog, a setting | [`docs/user-guide.md`](docs/user-guide.md) **and** [`docs/benutzerhandbuch.md`](docs/benutzerhandbuch.md) — the two language versions are only useful if both move together |
| Component boundaries, data flow, storage layout, startup order | [`docs/architecture.md`](docs/architecture.md) |
| Public types or their signatures | [`docs/api-reference.md`](docs/api-reference.md) |
| Build, test, debug, conventions, analyzer rules | [`docs/developer-guide.md`](docs/developer-guide.md) |
| A new failure mode, exit code, or known limitation | [`docs/troubleshooting.md`](docs/troubleshooting.md) |
| Versioning, tagging, the release pipeline | [`docs/release-process.md`](docs/release-process.md) |
| A decision that constrains future work | a new [ADR](docs/adr/README.md), added to its index |
| Anything at all | a `CHANGELOG.md` entry under `[Unreleased]` |

If you are unsure whether a document is affected, open it and read the section your change touches. That takes a minute and is the whole cost of keeping this project's documentation trustworthy.

## Tests

xUnit with Moq and FluentAssertions. FluentAssertions is pinned below 8 — that release changed to a commercial licence.

- Tests run headless. Nothing creates a WPF `Application` or needs an STA thread; view models are exercised directly.
- If you change the dependency injection graph, extend `CompositionRootTests`.
- Line coverage currently sits at **69.2 %**, and CI **fails below 65 %**. The floor is deliberately under the current figure so coverage can only ratchet upwards; raise it when the real number does. It also fails on 0 %, because a broken collector reads exactly like an untested code base — which is what happened until v0.4.10, when `DebugType=none` in Release left coverlet with no debug information and every report came out empty.
- Avoid fixed sleeps. Poll with a generous deadline and exit early — slow CI runners have already broken timing-sensitive tests once.

## Documentation

Documentation lives in [`docs/`](docs). Decisions that constrain future work get an [ADR](docs/adr/README.md) — the index explains when to write one and includes a template.

## Reporting issues

Use the [issue templates](https://github.com/eXORR6077/taskbar-grouping/issues/new/choose). For bugs, attach the relevant log from `%APPDATA%\TaskbarFolders\logs\`; it is far more useful than a screenshot. See [SUPPORT.md](SUPPORT.md).

Security issues do not go in the tracker — see [SECURITY.md](SECURITY.md).

## Code of Conduct

Participation is governed by the [Code of Conduct](CODE_OF_CONDUCT.md).
