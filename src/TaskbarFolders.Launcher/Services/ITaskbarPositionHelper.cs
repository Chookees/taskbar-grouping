using System.Windows;
using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Launcher.Services;

/// <summary>
/// Locates the Windows taskbar and computes a popup placement adjacent to it on the
/// monitor under the cursor. Honours <see cref="PopupPositionPreference"/> for users
/// who want to force a side regardless of the taskbar edge.
/// </summary>
public interface ITaskbarPositionHelper
{
    /// <summary>
    /// Computes the desired top-left of a popup window with the supplied size.
    /// </summary>
    /// <param name="popupSize">Width and height of the popup in device-independent pixels.</param>
    /// <param name="preference">User preference for vertical anchoring (Auto/Above/Below).</param>
    PopupPlacement ComputePlacement(Size popupSize, PopupPositionPreference preference);
}
