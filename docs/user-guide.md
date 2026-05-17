# User Guide

> ⚠️ **Status: Pre-Alpha (v0.1.x).** This guide describes the target product. **None of the user-facing functionality below is implemented yet.** First functional release is planned for v0.2.0. See the [README](../README.md) for the current state.

## Installation 🚧

### Using the Installer

1. Download `TaskbarFolders-Setup.exe` from the [Releases page](https://github.com/TODO-GH-OWNER/TaskbarFolders/releases)
2. Run the installer
3. Follow the setup wizard
4. Launch **TaskbarFolders Manager** from the Start Menu

### Portable Version

1. Download `TaskbarFolders-portable.zip`
2. Extract to any folder
3. Run `TaskbarFolders.Manager.exe`

## Creating a Group

1. Open **TaskbarFolders Manager**
2. Click the **"+ New Group"** button
3. Enter a name for your group (e.g., "Dev Tools", "Games", "Office")
4. The group editor opens

## Adding Apps to a Group

1. In the group editor, drag and drop `.exe` or `.lnk` files from Windows Explorer into the app list
2. Alternatively, click **"Add App"** and browse for an executable
3. The app's icon and name are detected automatically
4. You can rename apps or change their icons manually

## Composite Icon Preview

As you add apps, the 2x2 composite icon updates in real-time:
- 1 app: Single icon centered
- 2 apps: Two icons side by side
- 3 apps: Three icons in an L-shape
- 4+ apps: First four icons in a 2x2 grid

## Pinning to Taskbar

1. After saving a group, find the generated launcher in the group list
2. Right-click the group and select **"Open file location"**
3. Right-click the `.exe` file and select **"Pin to Taskbar"**
4. The group icon now appears in your taskbar

## Using a Group

1. Click the group icon in the taskbar
2. A popup appears with all your grouped apps
3. Click any app to launch it
4. The popup closes automatically
5. Click outside the popup to dismiss it

## Settings

### Theme
- **System**: Follows Windows dark/light mode
- **Light**: Always light theme
- **Dark**: Always dark theme

### Autostart
Enable to start TaskbarFolders Manager with Windows.

### Animations
Toggle popup open/close animations.

### Popup Position
- **Auto**: Popup appears near the clicked taskbar icon
- **Above**: Always above the taskbar
- **Below**: Always below the taskbar

## Editing a Group

1. Open **TaskbarFolders Manager**
2. Click on an existing group
3. Add, remove, or reorder apps
4. Changes are saved automatically
5. The composite icon updates when you modify the group

## Deleting a Group

1. Open **TaskbarFolders Manager**
2. Right-click a group
3. Select **"Delete Group"**
4. Confirm the deletion
5. Unpin the group from the taskbar manually if needed

## Uninstallation

### Installer Version
Use **"Add or Remove Programs"** in Windows Settings.

### Portable Version
Simply delete the extracted folder.
