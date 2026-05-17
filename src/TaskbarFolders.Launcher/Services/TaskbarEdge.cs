namespace TaskbarFolders.Launcher.Services;

/// <summary>Screen edge on which the Windows taskbar is anchored.</summary>
public enum TaskbarEdge
{
    /// <summary>Anchored to the left of the screen.</summary>
    Left = 0,

    /// <summary>Anchored to the top of the screen.</summary>
    Top = 1,

    /// <summary>Anchored to the right of the screen.</summary>
    Right = 2,

    /// <summary>Anchored to the bottom of the screen (Windows default).</summary>
    Bottom = 3,
}
