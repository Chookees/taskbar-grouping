# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.0] - 2026-05-06

### Added

- **Core: Icon Engine**
  - Icon extraction from `.exe`, `.lnk`, `.ico` via Windows Shell API (SHGetFileInfo, ExtractIconEx)
  - Shortcut (.lnk) resolution via WScript.Shell COM
  - 2x2 composite icon generation with rounded background
  - Multi-resolution `.ico` writer (16/32/48/256 PNG entries)
  - In-memory + disk icon cache with SHA256 key normalization

- **Shared: Models & Configuration**
  - Data models: AppEntry, GroupConfig, AppSettings
  - JSON-based configuration persistence in `%APPDATA%/TaskbarFolders/`
  - Group configs stored as individual JSON files per group
  - Path utility with directory management and filename sanitization

- **Manager: WPF Main Application**
  - MVVM architecture with DI (Microsoft.Extensions.DependencyInjection)
  - Sidebar/content master-detail layout
  - Group editor with drag & drop support for `.exe`/`.lnk` files
  - Live composite icon preview
  - Settings view (autostart, theme, animations, popup position)
  - Light and dark theme resource dictionaries
  - Shortcut (.lnk) generation with composite icon on save
  - Explorer integration ("Ordner offnen" opens shortcut location)
  - Full cleanup on group delete (config, icon, shortcuts)

- **Launcher: Popup Application**
  - Borderless popup window with rounded corners and drop shadow
  - Dynamic grid layout based on group column configuration
  - Smart positioning relative to taskbar edge (top/bottom/left/right)
  - Focus-loss auto-close behavior
  - Application launching via Process.Start with ShellExecute

- **Build & CI/CD**
  - Solution with 4 source projects and 3 test projects
  - 30 unit tests covering icon generation, caching, .ico writing, and configuration
  - GitHub Actions CI: build, test, format check, coverage upload
  - GitHub Actions Release: self-contained single-file publish, Inno Setup installer, portable ZIP, GitHub Release
  - CodeQL security analysis (weekly + on push)
  - Dependabot for NuGet and GitHub Actions dependencies

- **Installer**
  - Inno Setup with German and English language support
  - Single-directory install (Manager + Launcher side by side)
  - Optional autostart registry entry
  - Optional desktop shortcut
  - AppData cleanup on uninstall
