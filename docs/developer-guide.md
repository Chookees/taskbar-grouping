# Developer Guide

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows 10 version 1903 or later / Windows 11
- IDE: Visual Studio 2022, JetBrains Rider, or VS Code with C# Dev Kit

## Getting Started

```bash
git clone https://github.com/YOUR_USER/TaskbarFolders.git
cd TaskbarFolders
dotnet restore
dotnet build --configuration Release
```

## Project Structure

| Project | Type | Purpose |
|---|---|---|
| `TaskbarFolders.Core` | Class Library | Icon extraction, composite generation, .ico writing, caching |
| `TaskbarFolders.Shared` | Class Library | Models, DTOs, JSON configuration persistence, path utilities |
| `TaskbarFolders.Manager` | WPF App (WinExe) | Main UI for group management, shortcut generation |
| `TaskbarFolders.Launcher` | WPF App (WinExe) | Lightweight popup per group, launched via shortcuts |

### Test Projects

| Project | Tests | Coverage |
|---|---|---|
| `TaskbarFolders.Core.Tests` | 28 | Icon generation, .ico writing, caching, config persistence |
| `TaskbarFolders.Launcher.Tests` | 1 | Smoke test |
| `TaskbarFolders.Manager.Tests` | 1 | Smoke test |

## Building

```bash
# Debug build
dotnet build

# Release build
dotnet build --configuration Release

# Publish self-contained single-file (both apps)
dotnet publish src/TaskbarFolders.Manager/TaskbarFolders.Manager.csproj \
  --configuration Release --runtime win-x64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
  --output ./publish/Manager

dotnet publish src/TaskbarFolders.Launcher/TaskbarFolders.Launcher.csproj \
  --configuration Release --runtime win-x64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
  --output ./publish/Launcher
```

## Running Tests

```bash
# Run all tests
dotnet test --configuration Release

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Specific project
dotnet test tests/TaskbarFolders.Core.Tests
```

## Code Style

The project uses `.editorconfig` for style enforcement and Roslyn analyzers via `Directory.Build.props`. All warnings are treated as errors.

```bash
# Check formatting
dotnet format --verify-no-changes

# Fix formatting
dotnet format
```

### Key Conventions

- **File-scoped namespaces**: `namespace Foo.Bar;`
- **Private fields**: `_camelCase` prefix
- **XML documentation**: Required on all public members (CS1591)
- **MVVM**: ViewModels in `ViewModels/`, Views in `Views/`, minimal code-behind
- **DI**: Register services in `App.xaml.cs`, inject via constructor
- **Async**: All I/O operations are async with `ConfigureAwait(true)` for UI context
- **P/Invoke**: Use `LibraryImport` (source-generated) where possible, `DllImport` for complex struct marshalling

### Analyzer Suppressions

Test projects suppress these rules via `tests/Directory.Build.props`:
- `CA1707` -- Underscores in test method names
- `CS1591` -- XML documentation requirement
- `CA1859` -- Concrete return types for private methods
- `CA1816` -- GC.SuppressFinalize
- `CA1869` -- Static JsonSerializerOptions

## Architecture

See [architecture.md](architecture.md) for the full system overview.

### Key Patterns

- **MVVM**: Views bind to ViewModels via DataContext; DataTemplates select views by ViewModel type
- **Dependency Injection**: `Microsoft.Extensions.DependencyInjection` in both Manager and Launcher
- **Repository Pattern**: `IGroupConfigStore` / `IAppSettingsStore` abstract JSON persistence
- **Attached Behaviors**: `FileDragDropBehavior`, `FocusLossBehavior` add functionality without code-behind
- **Resource Dictionaries**: `LightTheme.xaml` / `DarkTheme.xaml` for theme switching

## Debugging

### Manager App
```bash
dotnet run --project src/TaskbarFolders.Manager
```

### Launcher App (with group ID)
```bash
# Use a valid group ID from %APPDATA%/TaskbarFolders/groups/
dotnet run --project src/TaskbarFolders.Launcher -- --group-id <guid>
```

### Common Issues

| Issue | Cause | Fix |
|---|---|---|
| Icon extraction returns null | Target .exe doesn't exist or is inaccessible | Verify file path and permissions |
| CA1822 on DI services | Method doesn't access instance data | Suppress with `SuppressMessage` if the method is designed for DI |
| SYSLIB1062 AllowUnsafeBlocks | LibraryImport requires unsafe context | Already set in Core and Launcher .csproj |
| SYSLIB1051 struct marshalling | Complex structs not supported by LibraryImport | Use classic `DllImport` instead |
| Popup positions incorrectly | Multi-monitor or unusual taskbar placement | Check `TaskbarPositionHelper` logic |

## Adding a New Feature

1. Create a branch: `git checkout -b feature/my-feature develop`
2. Implement following MVVM and DI patterns
3. Add unit tests
4. Run `dotnet format` and `dotnet test`
5. Commit with Conventional Commits: `feat(scope): description`
6. Open a PR against `develop`

## Release Process

1. Merge `develop` into `main`
2. Tag with semantic version: `git tag v0.2.0`
3. Push tag: `git push origin v0.2.0`
4. GitHub Actions builds, tests, publishes, and creates a GitHub Release with installer + portable ZIP
