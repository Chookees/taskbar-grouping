# TaskbarFolders

[![CI](https://github.com/YOUR_USER/TaskbarFolders/actions/workflows/ci.yml/badge.svg)](https://github.com/YOUR_USER/TaskbarFolders/actions/workflows/ci.yml)
[![Release](https://github.com/YOUR_USER/TaskbarFolders/actions/workflows/release.yml/badge.svg)](https://github.com/YOUR_USER/TaskbarFolders/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

**iOS-style taskbar groups for Windows 11.** Group your apps into folders, pin them to the taskbar, and launch them from a beautiful popup.

![TaskbarFolders Screenshot](assets/screenshots/preview.png)

## Features

- **Group apps into folders** – Drag & drop `.exe` and `.lnk` files into customizable groups
- **Automatic composite icons** – Each group gets a 2×2 preview icon generated from the contained app icons
- **Pin to taskbar** – Every group is a standalone `.exe` that can be pinned to the Windows taskbar
- **Animated popup** – Click a group to reveal apps in a rounded-corner popup with blur background
- **Light/Dark/System themes** – Follows your Windows theme or set per group
- **Multi-monitor support** – Popup positions correctly on any monitor setup
- **High-DPI aware** – Crisp rendering at all scaling levels (100%–200%)

## Installation

### Installer (recommended)

1. Download the latest `TaskbarFolders-Setup.exe` from [Releases](https://github.com/YOUR_USER/TaskbarFolders/releases)
2. Run the installer and follow the instructions
3. Launch **TaskbarFolders Manager** from the Start Menu

### Portable

1. Download the latest `TaskbarFolders-portable.zip` from [Releases](https://github.com/YOUR_USER/TaskbarFolders/releases)
2. Extract to any folder
3. Run `TaskbarFolders.Manager.exe`

## Quick Start

1. Open **TaskbarFolders Manager**
2. Click **New Group** and give it a name (e.g., "Dev Tools")
3. Drag & drop your favorite apps into the group
4. The composite icon is generated automatically
5. Right-click the generated `.exe` in the group list and select **Pin to Taskbar**
6. Click the pinned icon to open the popup and launch any app

## Building from Source

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows 10/11

### Build

```bash
git clone https://github.com/YOUR_USER/TaskbarFolders.git
cd TaskbarFolders
dotnet restore
dotnet build --configuration Release
```

### Run Tests

```bash
dotnet test --configuration Release
```

### Run the Manager

```bash
dotnet run --project src/TaskbarFolders.Manager
```

## Tech Stack

| Component | Technology |
|---|---|
| Language | C# (.NET 10) |
| UI Framework | WPF (Windows Presentation Foundation) |
| Architecture | MVVM |
| DI Container | Microsoft.Extensions.DependencyInjection |
| Tests | xUnit + Moq + FluentAssertions |
| Installer | Inno Setup |
| CI/CD | GitHub Actions |

## Project Structure

```
TaskbarFolders/
├── src/
│   ├── TaskbarFolders.Core/        # Icon engine, extraction, composite generation
│   ├── TaskbarFolders.Shared/      # Models, DTOs, configuration, utilities
│   ├── TaskbarFolders.Manager/     # WPF main app (group management)
│   └── TaskbarFolders.Launcher/    # Lightweight popup app (per group)
├── tests/                          # xUnit test projects
├── docs/                           # Documentation
├── installer/                      # Inno Setup script
└── assets/                         # Icons and screenshots
```

## Documentation

- [Architecture Overview](docs/architecture.md)
- [User Guide](docs/user-guide.md)
- [Developer Guide](docs/developer-guide.md)
- [API Reference](docs/api-reference.md)

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on how to contribute to this project.

## License

This project is licensed under the MIT License – see the [LICENSE](LICENSE) file for details.
