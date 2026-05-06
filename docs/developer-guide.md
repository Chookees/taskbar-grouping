# Developer Guide

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10 version 1903 or later / Windows 11
- IDE: Visual Studio 2022, JetBrains Rider, or VS Code with C# Dev Kit

## Getting Started

```bash
git clone https://github.com/YOUR_USER/TaskbarFolders.git
cd TaskbarFolders
dotnet restore
dotnet build
```

## Project Structure

| Project | Type | Purpose |
|---|---|---|
| `TaskbarFolders.Core` | Class Library | Icon extraction, composite generation, caching |
| `TaskbarFolders.Shared` | Class Library | Models, DTOs, configuration, utilities |
| `TaskbarFolders.Manager` | WPF App | Main UI for group management |
| `TaskbarFolders.Launcher` | WPF App | Lightweight popup per group |

## Building

```bash
# Debug build
dotnet build

# Release build
dotnet build --configuration Release

# Publish self-contained
dotnet publish src/TaskbarFolders.Manager/TaskbarFolders.Manager.csproj \
  --configuration Release --runtime win-x64 --self-contained true \
  -p:PublishSingleFile=true --output ./publish
```

## Running Tests

```bash
# Run all tests
dotnet test

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Specific project
dotnet test tests/TaskbarFolders.Core.Tests
```

## Code Style

The project uses `.editorconfig` for style enforcement and Roslyn analyzers via `Directory.Build.props`.

```bash
# Check formatting
dotnet format --verify-no-changes

# Fix formatting
dotnet format
```

### Key Conventions

- **File-scoped namespaces**: `namespace Foo.Bar;`
- **Private fields**: `_camelCase` prefix
- **XML documentation**: Required on all public members
- **MVVM**: ViewModels in `ViewModels/`, Views in `Views/`, no code-behind logic
- **DI**: Register services in `App.xaml.cs`, inject via constructor
- **Async**: All I/O operations must be async

## Architecture

See [architecture.md](architecture.md) for the full system overview.

### Key Patterns

- **MVVM**: Views bind to ViewModels via DataContext
- **Dependency Injection**: `Microsoft.Extensions.DependencyInjection`
- **Repository Pattern**: `IGroupConfigStore` abstracts JSON persistence
- **Strategy Pattern**: `IIconExtractor` allows different extraction methods

## Debugging

### Manager App
```bash
dotnet run --project src/TaskbarFolders.Manager
```

### Launcher App (with group ID)
```bash
dotnet run --project src/TaskbarFolders.Launcher -- --group-id <id>
```

### Common Issues

- **Icon extraction fails**: Ensure the target .exe exists and is accessible
- **Popup positioning wrong**: Check `TaskbarPositionHelper` for multi-monitor logic
- **Build warnings as errors**: Fix all warnings or adjust `Directory.Build.props`

## Adding a New Feature

1. Create a branch: `git checkout -b feature/my-feature develop`
2. Implement the feature following MVVM and DI patterns
3. Add unit tests (target: 70% coverage)
4. Run `dotnet format` and `dotnet test`
5. Open a PR against `develop`
