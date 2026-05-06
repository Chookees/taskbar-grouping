; TaskbarFolders Inno Setup Script

#define MyAppName "TaskbarFolders"
#define MyAppVersion "0.1.0"
#define MyAppPublisher "TaskbarFolders Contributors"
#define MyAppURL "https://github.com/YOUR_USER/TaskbarFolders"
#define MyAppExeName "TaskbarFolders.Manager.exe"
#define MyLauncherExeName "TaskbarFolders.Launcher.exe"

[Setup]
AppId={{B8F4E2A1-3C5D-4E6F-A7B8-9C0D1E2F3A4B}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=..\LICENSE
OutputDir=Output
OutputBaseFilename=TaskbarFolders-{#MyAppVersion}-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.17763
UninstallDisplayIcon={app}\{#MyAppExeName}
VersionInfoVersion={#MyAppVersion}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "autostart"; Description: "Start with Windows"; GroupDescription: "Other:"

[Files]
; Both Manager and Launcher install to the same directory (single-file publish)
Source: "..\publish\Manager\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\publish\Launcher\{#MyLauncherExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "TaskbarFolders"; ValueData: """{app}\{#MyAppExeName}"""; Flags: uninsdeletevalue; Tasks: autostart

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Clean up generated files in AppData on uninstall
Type: filesandirs; Name: "{userappdata}\TaskbarFolders\icons"
Type: filesandirs; Name: "{userappdata}\TaskbarFolders\launchers"
Type: dirifempty; Name: "{userappdata}\TaskbarFolders"
