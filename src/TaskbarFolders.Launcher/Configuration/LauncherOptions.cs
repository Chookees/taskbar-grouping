namespace TaskbarFolders.Launcher.Configuration;

/// <summary>
/// Runtime options parsed from the launcher command line.
/// </summary>
/// <param name="GroupId">Identifier of the taskbar group whose popup should be displayed.</param>
public sealed record LauncherOptions(string GroupId);
