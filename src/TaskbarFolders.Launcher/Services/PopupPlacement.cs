namespace TaskbarFolders.Launcher.Services;

/// <summary>
/// Computed target placement for the popup window expressed in screen (device-independent) pixels.
/// </summary>
/// <param name="Left">Left coordinate on the virtual desktop.</param>
/// <param name="Top">Top coordinate on the virtual desktop.</param>
public readonly record struct PopupPlacement(double Left, double Top);
