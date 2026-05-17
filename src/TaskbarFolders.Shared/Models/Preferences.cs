namespace TaskbarFolders.Shared.Models;

/// <summary>Theme preference for the Manager UI and per-group popup.</summary>
public enum ThemePreference
{
    /// <summary>Follow the Windows system theme (default).</summary>
    System = 0,

    /// <summary>Always render the light variant.</summary>
    Light = 1,

    /// <summary>Always render the dark variant.</summary>
    Dark = 2,
}

/// <summary>Where the launcher popup is anchored relative to the taskbar icon.</summary>
public enum PopupPositionPreference
{
    /// <summary>Pick automatically based on the taskbar edge (default).</summary>
    Auto = 0,

    /// <summary>Always render the popup above the taskbar.</summary>
    Above = 1,

    /// <summary>Always render the popup below the taskbar.</summary>
    Below = 2,
}
