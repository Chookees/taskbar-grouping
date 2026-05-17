# TaskbarFolders

[![CI](https://github.com/eXORR6077/taskbar-grouping/actions/workflows/ci.yml/badge.svg)](https://github.com/eXORR6077/taskbar-grouping/actions/workflows/ci.yml)
[![Release](https://github.com/eXORR6077/taskbar-grouping/actions/workflows/release.yml/badge.svg)](https://github.com/eXORR6077/taskbar-grouping/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

> ℹ️ **Status: v0.2.0.** First functional release — all features below are implemented. See [CHANGELOG.md](CHANGELOG.md) for the per-version detail.

**iOS-style taskbar groups for Windows 11.** Group your apps into folders, pin them to the taskbar, and launch them from a beautiful popup.

<!-- TODO: Add screenshot at assets/screenshots/preview.png once a polished build is ready. -->

## Features

- ✅ **Group apps into folders** – Drag & drop `.exe` and `.lnk` files into customizable groups
- ✅ **Automatic composite icons** – Each group gets a 2×2 preview icon generated from the contained app icons
- ✅ **Pin to taskbar** – Each group gets its own `.lnk` shortcut (with a distinct AppUserModelID) that you can pin to the Windows taskbar
- ✅ **Animated popup** – Click a pinned group to reveal apps in a rounded-corner popup with Acrylic backdrop on Windows 11 22H2+
- ✅ **Light/Dark/System themes** – Follows your Windows theme or set per group; live-switches when Windows theme changes
- ✅ **Multi-monitor support** – Popup positions itself adjacent to the taskbar on the monitor under the cursor; handles secondary monitors with negative X
- ✅ **High-DPI aware** – Per-monitor V2 DPI awareness so the UI stays crisp on mixed-DPI setups (100 %–200 %)

## Installation

### Installer (recommended)

1. Download the latest `TaskbarFolders-Setup.exe` from [Releases](https://github.com/eXORR6077/taskbar-grouping/releases)
2. Run the installer and follow the instructions
3. Launch **TaskbarFolders Manager** from the Start Menu

### Portable

1. Download the latest `TaskbarFolders-portable.zip` from [Releases](https://github.com/eXORR6077/taskbar-grouping/releases)
2. Extract to any folder
3. Run `TaskbarFolders.Manager.exe`

## Quick Start

1. Open **TaskbarFolders Manager**
2. Click **+ Add** in the sidebar and give the group a name (e.g., "Dev Tools")
3. Drag & drop `.exe` or `.lnk` files into the group editor — the composite icon updates live
4. Click **Show shortcut...** to open the generated `.lnk` in Explorer
5. Right-click the `.lnk` → **Show more options** → **Pin to taskbar** (Win11 22H2+) or **Pin to taskbar** directly (older Win10/11)
6. Click the pinned tile to open the popup and launch any app

## Building from Source

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10/11

### Build

```bash
git clone https://github.com/eXORR6077/taskbar-grouping.git
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
| Language | C# (.NET 8) |
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
