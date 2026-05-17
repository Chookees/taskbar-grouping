# API Reference

> ℹ️ **Status: v0.2.0.** Every type below exists in code; the file links lead to the actual source.

## TaskbarFolders.Core

### Namespace: `TaskbarFolders.Core.Icons`

#### `IIconExtractor` ✅ (interface only — no implementation yet)

Extracts icons from executable files, shortcuts, and icon files.

```csharp
public interface IIconExtractor
{
    BitmapSource? ExtractIcon(string filePath, int size = 256);
}
```

**Parameters:**
- `filePath`: Path to an `.exe`, `.lnk`, or `.ico` file
- `size`: Desired icon size in pixels (default: 256)

**Returns:** The extracted icon as a `BitmapSource`, or `null` if extraction fails.

---

#### `ICompositeIconGenerator` ✅ (interface only — no implementation yet)

Generates composite icons from multiple source icons in a grid layout.

```csharp
public interface ICompositeIconGenerator
{
    BitmapSource GenerateComposite(IReadOnlyList<BitmapSource> icons, int outputSize = 256);
}
```

**Parameters:**
- `icons`: Source icons to compose (up to 4)
- `outputSize`: Output icon size in pixels (default: 256)

**Returns:** The composite icon as a `BitmapSource`.

---

## TaskbarFolders.Shared

### Namespace: `TaskbarFolders.Shared.Models`

#### `AppEntry` ✅

Represents a single application entry within a taskbar group.

| Property | Type | Description |
|---|---|---|
| `Name` | `string` | Display name of the application |
| `Path` | `string` | Full path to the executable or shortcut |
| `IconPath` | `string?` | Optional custom icon path |
| `Arguments` | `string?` | Command-line arguments (default: null; omitted from JSON when null) |

---

#### `GroupConfig` ✅

Configuration for a single taskbar group.

| Property | Type | Description |
|---|---|---|
| `Id` | `string` | Unique identifier (auto-generated) |
| `GroupName` | `string` | Display name of the group |
| `Columns` | `int` | Popup grid columns. Clamped to [1..6] on assignment (default: 3) |
| `Theme` | `ThemePreference` | Theme override; JSON values: `system`, `light`, `dark` (default: `system`) |
| `Apps` | `List<AppEntry>` | Applications in this group |

---

#### `AppSettings` ✅

Global application settings.

| Property | Type | Description |
|---|---|---|
| `AutoStart` | `bool` | Start on Windows startup (default: false) |
| `Theme` | `ThemePreference` | Global theme; JSON values: `system`, `light`, `dark` (default: `system`) |
| `EnableAnimations` | `bool` | Popup animations (default: true) |
| `PopupPosition` | `PopupPositionPreference` | Popup position; JSON values: `auto`, `above`, `below` (default: `auto`) |

---

### Namespace: `TaskbarFolders.Shared.Configuration` ✅

#### `IGroupConfigStore` ✅

Interface for persisting group configurations.

```csharp
public interface IGroupConfigStore
{
    Task<IReadOnlyList<GroupConfig>> LoadAllAsync();
    Task<GroupConfig?> LoadAsync(string groupId);
    Task SaveAsync(GroupConfig config);
    Task DeleteAsync(string groupId);
}
```

#### `IAppSettingsStore` ✅

Interface for persisting application settings.

```csharp
public interface IAppSettingsStore
{
    Task<AppSettings> LoadAsync();
    Task SaveAsync(AppSettings settings);
}
```
