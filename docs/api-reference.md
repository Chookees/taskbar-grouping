# API Reference

## TaskbarFolders.Core

### Namespace: `TaskbarFolders.Core.Icons`

#### `IIconExtractor`

Extracts icons from executable files, shortcuts, and icon files.

```csharp
public interface IIconExtractor
{
    BitmapSource? ExtractIcon(string filePath, int size = 256);
}
```

**Implementation:** `ShellIconExtractor` -- uses ExtractIconEx, SHGetFileInfo, and IconBitmapDecoder as fallback chain. Resolves `.lnk` shortcuts via WScript.Shell COM.

---

#### `ICompositeIconGenerator`

Generates composite icons from multiple source icons in a 2x2 grid layout.

```csharp
public interface ICompositeIconGenerator
{
    BitmapSource GenerateComposite(IReadOnlyList<BitmapSource> icons, int outputSize = 256);
}
```

**Implementation:** `CompositeIconGenerator` -- uses `DrawingVisual` with a rounded-rectangle background. Takes up to 4 icons and arranges them in a grid with padding.

---

#### `IcoWriter`

Writes `BitmapSource` images as multi-resolution `.ico` files.

```csharp
public static class IcoWriter
{
    static void Write(BitmapSource source, string outputPath);
    static void Write(BitmapSource source, Stream outputStream);
}
```

Generates ICO files with PNG-encoded entries at 16x16, 32x32, 48x48, and 256x256 pixels. Creates parent directories if they don't exist.

---

#### `IconCache`

Thread-safe in-memory icon cache with optional disk persistence.

```csharp
public sealed class IconCache
{
    BitmapSource? GetOrCreate(string key, Func<BitmapSource?> factory);
    void Invalidate(string key);
    void Clear();
}
```

Uses `ConcurrentDictionary` for thread safety. Keys are SHA256-normalized. Disk cache stores PNG files.

---

## TaskbarFolders.Shared

### Namespace: `TaskbarFolders.Shared.Models`

#### `AppEntry`

Represents a single application entry within a taskbar group.

| Property | Type | Default | Description |
|---|---|---|---|
| `Name` | `string` | required | Display name of the application |
| `Path` | `string` | required | Full path to the executable or shortcut |
| `IconPath` | `string?` | `null` | Optional custom icon path (falls back to Path) |
| `Arguments` | `string` | `""` | Command-line arguments |

#### `GroupConfig`

Configuration for a single taskbar group.

| Property | Type | Default | Description |
|---|---|---|---|
| `Id` | `string` | auto-generated GUID | Unique identifier |
| `GroupName` | `string` | required | Display name of the group |
| `Columns` | `int` | `3` | Popup grid columns |
| `Theme` | `string` | `"system"` | Theme: system, light, dark |
| `Apps` | `List<AppEntry>` | `[]` | Applications in this group |

#### `AppSettings`

Global application settings.

| Property | Type | Default | Description |
|---|---|---|---|
| `AutoStart` | `bool` | `false` | Start on Windows startup |
| `Theme` | `string` | `"system"` | Global theme |
| `EnableAnimations` | `bool` | `true` | Popup animations |
| `PopupPosition` | `string` | `"auto"` | Popup position: auto, above, below |

---

### Namespace: `TaskbarFolders.Shared.Configuration`

#### `IGroupConfigStore`

Interface for persisting group configurations as JSON files.

```csharp
public interface IGroupConfigStore
{
    Task<IReadOnlyList<GroupConfig>> LoadAllAsync();
    Task<GroupConfig?> LoadAsync(string groupId);
    Task SaveAsync(GroupConfig config);
    Task DeleteAsync(string groupId);
}
```

**Implementation:** `JsonGroupConfigStore` -- stores one JSON file per group in `%APPDATA%/TaskbarFolders/groups/`. Uses camelCase JSON naming policy.

#### `IAppSettingsStore`

Interface for persisting application settings.

```csharp
public interface IAppSettingsStore
{
    Task<AppSettings> LoadAsync();
    Task SaveAsync(AppSettings settings);
}
```

**Implementation:** `JsonAppSettingsStore` -- single `settings.json` file in `%APPDATA%/TaskbarFolders/`.

---

### Namespace: `TaskbarFolders.Shared.Utilities`

#### `PathHelper`

Centralized path constants for all application directories and files.

| Member | Returns | Description |
|---|---|---|
| `GroupsDirectory` | `string` | `%APPDATA%/TaskbarFolders/groups/` |
| `IconsDirectory` | `string` | `%APPDATA%/TaskbarFolders/icons/` |
| `LaunchersDirectory` | `string` | `%APPDATA%/TaskbarFolders/launchers/` |
| `SettingsFilePath` | `string` | `%APPDATA%/TaskbarFolders/settings.json` |
| `GetGroupFilePath(id)` | `string` | Path to group JSON file |
| `GetGroupIconPath(id)` | `string` | Path to group .ico file |
| `GetGroupShortcutPath(id, name)` | `string` | Path to group .lnk shortcut |
| `EnsureDirectoriesExist()` | `void` | Creates all required directories |

---

## TaskbarFolders.Manager

### Namespace: `TaskbarFolders.Manager.Services`

#### `LauncherGenerator`

Generates composite icons, `.lnk` shortcuts, and manages launcher files per group.

```csharp
public sealed class LauncherGenerator
{
    void GenerateGroupIcon(string groupId, BitmapSource compositeIcon);
    string? GenerateShortcut(string groupId, string groupName);
    void DeleteGroupFiles(string groupId);
}
```

- `GenerateGroupIcon` writes the composite as a multi-resolution .ico via `IcoWriter`
- `GenerateShortcut` creates a .lnk shortcut pointing to Launcher.exe with `--group-id` argument and the composite icon; returns the shortcut path or null if Launcher.exe was not found
- `DeleteGroupFiles` removes the .ico and all matching .lnk files for a group

---

## TaskbarFolders.Launcher

### Namespace: `TaskbarFolders.Launcher.Services`

#### `TaskbarPositionHelper`

Determines popup window placement relative to the Windows taskbar.

```csharp
public static partial class TaskbarPositionHelper
{
    static void PositionWindow(Window window, double width, double height);
}
```

Uses `SHAppBarMessage` to detect taskbar edge (top/bottom/left/right) and `GetCursorPos` for cursor-relative positioning.

#### `ProcessLauncher`

Launches applications from popup clicks.

```csharp
public sealed class ProcessLauncher
{
    void Launch(string path, string arguments = "");
}
```

Uses `Process.Start` with `UseShellExecute = true` for proper shell verb handling.
