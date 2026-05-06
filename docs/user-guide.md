# User Guide

## Installation

### Using the Installer

1. Download `TaskbarFolders-*-Setup.exe` from the [Releases page](https://github.com/YOUR_USER/TaskbarFolders/releases)
2. Run the installer
3. Choose language (German or English)
4. Follow the setup wizard
5. Launch **TaskbarFolders** from the Start Menu

### Portable Version

1. Download `TaskbarFolders-*-portable.zip`
2. Extract to any folder
3. Run `TaskbarFolders.Manager.exe`

## Creating a Group

1. Open **TaskbarFolders Manager**
2. Click **+ Neue Gruppe** in the sidebar
3. A new group called "New Group" appears and the editor opens
4. Enter a name for your group (e.g., "Dev Tools", "Games", "Office")
5. Choose the number of columns for the popup grid (2--5)

## Adding Apps to a Group

1. In the group editor, drag `.exe` or `.lnk` files from Windows Explorer onto the drop zone
2. The app's icon and name are detected automatically
3. You can edit the display name inline
4. The file path is shown below each app entry
5. Click the **x** button to remove an app

## Composite Icon Preview

As you add apps, the 2x2 composite icon updates in real-time in the editor:
- 1 app: Single icon
- 2 apps: Two icons
- 3 apps: Three icons
- 4+ apps: First four icons in a 2x2 grid

The composite icon has a rounded background and is used as the shortcut icon on the taskbar.

## Saving and Pinning to Taskbar

1. Click **Speichern** to save the group
2. A confirmation panel appears showing the shortcut path
3. Click **Ordner offnen** to open the folder containing the shortcut in Explorer
4. Drag the `.lnk` shortcut file onto your Windows taskbar
5. The group icon now appears in your taskbar

## Using a Group

1. Click the group icon in the taskbar
2. A popup appears with all your grouped apps as a grid
3. Click any app icon to launch it
4. The popup closes automatically after launching
5. Click anywhere outside the popup to dismiss it

## Managing Groups

### Editing a Group

1. Click on a group in the sidebar
2. The group editor opens on the right
3. Add or remove apps, change the name or column count
4. Click **Speichern** to save changes and regenerate the shortcut

### Deleting a Group

1. Select a group in the sidebar
2. Click **Gruppe loschen** at the bottom of the sidebar
3. The group and all generated files (icon, shortcut) are removed
4. If the group was pinned to the taskbar, right-click the pin and choose "Unpin from taskbar"

## Settings

Click **Einstellungen** at the bottom of the sidebar to open the settings view.

### Theme
- **system**: Follows Windows dark/light mode
- **light**: Always light theme
- **dark**: Always dark theme

### Autostart
Enable to start TaskbarFolders Manager with Windows (adds to registry Run key).

### Animations
Toggle popup open/close animations on the Launcher.

### Popup Position
- **auto**: Popup appears near the cursor/taskbar
- **above**: Always above the taskbar
- **below**: Always below the taskbar

Click **Speichern** to save settings.

## Uninstallation

### Installer Version
Use **Settings > Apps > Installed Apps** in Windows 11 to uninstall. The uninstaller removes generated icons and shortcuts from AppData.

### Portable Version
Delete the extracted folder. Optionally delete `%APPDATA%\TaskbarFolders` to remove configuration data.
