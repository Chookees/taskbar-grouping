# TaskbarFolders

[![CI](https://github.com/YOUR_USER/TaskbarFolders/actions/workflows/ci.yml/badge.svg)](https://github.com/YOUR_USER/TaskbarFolders/actions/workflows/ci.yml)
[![Release](https://github.com/YOUR_USER/TaskbarFolders/actions/workflows/release.yml/badge.svg)](https://github.com/YOUR_USER/TaskbarFolders/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

**iOS-style taskbar groups for Windows 11.** Group your apps into folders, pin them to the taskbar, and launch them from a beautiful popup.

## Features

- **Group apps into folders** -- Drag & drop `.exe` and `.lnk` files into customizable groups
- **Automatic composite icons** -- Each group gets a 2x2 preview icon generated from the contained app icons
- **Pin to taskbar** -- Save a group to generate a `.lnk` shortcut you can drag onto the taskbar
- **Animated popup** -- Click a pinned group to reveal apps in a rounded-corner popup with drop shadow
- **Light & Dark themes** -- Switch between light and dark UI themes
- **Smart popup positioning** -- Popup appears near the taskbar on any edge (top, bottom, left, right)
- **High-DPI aware** -- Multi-resolution `.ico` files (16/32/48/256) for crisp rendering at all scaling levels

## Installation

### Installer (recommended)

1. Download the latest `TaskbarFolders-*-Setup.exe` from [Releases](https://github.com/YOUR_USER/TaskbarFolders/releases)
2. Run the installer and follow the instructions
3. Launch **TaskbarFolders** from the Start Menu

### Portable

1. Download the latest `TaskbarFolders-*-portable.zip` from [Releases](https://github.com/YOUR_USER/TaskbarFolders/releases)
2. Extract to any folder
3. Run `TaskbarFolders.Manager.exe`

## Quick Start

1. Open **TaskbarFolders Manager**
2. Click **+ Neue Gruppe** and give it a name
3. Drag & drop `.exe` or `.lnk` files onto the drop zone
4. The composite icon preview updates automatically
5. Click **Speichern** -- a shortcut is generated
6. Click **Ordner offnen** and drag the `.lnk` shortcut onto your taskbar
7. Click the pinned icon to open the popup and launch any app

## Building from Source

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows 10/11

### Build & Test

```bash
git clone https://github.com/YOUR_USER/TaskbarFolders.git
cd TaskbarFolders
dotnet build --configuration Release
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
| Icon Extraction | Windows Shell API (P/Invoke) |
| Tests | xUnit |
| Installer | Inno Setup |
| CI/CD | GitHub Actions |

## Project Structure

```
TaskbarFolders/
├── src/
│   ├── TaskbarFolders.Core/        # Icon extraction, composite generation, .ico writing
│   ├── TaskbarFolders.Shared/      # Models, configuration persistence, path utilities
│   ├── TaskbarFolders.Manager/     # WPF main app (group management UI)
│   └── TaskbarFolders.Launcher/    # Lightweight popup app (per-group launcher)
├── tests/                          # xUnit test projects (30 tests)
├── docs/                           # Architecture, user guide, developer guide, API reference
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

This project is licensed under the MIT License -- see the [LICENSE](LICENSE) file for details.
