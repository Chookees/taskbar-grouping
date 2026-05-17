# Contributing to TaskbarFolders

Thank you for your interest in contributing! This document provides guidelines and instructions for contributing.

## Development Setup

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10/11
- An IDE: Visual Studio 2022, JetBrains Rider, or VS Code with C# extension

### Getting Started

```bash
git clone https://github.com/eXORR6077/taskbar-grouping.git
cd TaskbarFolders
dotnet restore
dotnet build
dotnet test
```

## Branching Model

| Branch | Purpose |
|---|---|
| `main` | Stable releases (protected, PR only) |
| `develop` | Integration branch |
| `feature/<name>` | New features |
| `fix/<name>` | Bug fixes |
| `release/<version>` | Release preparation |

### Workflow

1. Fork the repository
2. Create a feature branch from `develop`: `git checkout -b feature/my-feature develop`
3. Make your changes
4. Ensure all tests pass: `dotnet test`
5. Ensure formatting is correct: `dotnet format --verify-no-changes`
6. Push your branch and open a Pull Request against `develop`

## Commit Conventions

We follow [Conventional Commits](https://www.conventionalcommits.org/).

**Format:** `<type>(<scope>): <description>`

### Types

| Type | Description |
|---|---|
| `feat` | A new feature |
| `fix` | A bug fix |
| `docs` | Documentation changes |
| `style` | Code style changes (formatting, no logic change) |
| `refactor` | Code refactoring (no feature or fix) |
| `test` | Adding or updating tests |
| `ci` | CI/CD pipeline changes |
| `chore` | Maintenance tasks |
| `perf` | Performance improvements |
| `build` | Build system changes |

### Examples

```
feat(icon-engine): add 2x2 composite icon generation
fix(launcher): resolve popup positioning on multi-monitor setups
docs(readme): add installation instructions
test(core): add unit tests for icon cache
```

## Code Style

- Follow the rules defined in `.editorconfig`
- Run `dotnet format` before committing
- All public members must have XML documentation comments
- Use file-scoped namespaces
- Follow MVVM pattern in WPF projects
- Use async/await for all I/O operations
- Use dependency injection – no `new` for services

## Testing

- Write tests using xUnit + FluentAssertions
- Use Moq for mocking dependencies
- Minimum code coverage target: 70%
- Run tests: `dotnet test --collect:"XPlat Code Coverage"`

## Pull Request Process

1. Fill out the PR template completely
2. Ensure CI passes (build, tests, formatting)
3. Request a review
4. Address review feedback
5. Squash and merge into `develop`

## Reporting Issues

Use the GitHub issue templates:
- **Bug Report** – for reporting bugs
- **Feature Request** – for suggesting new features

## Code of Conduct

Please read and follow our [Code of Conduct](CODE_OF_CONDUCT.md).
