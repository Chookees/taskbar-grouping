using System.Windows;

namespace TaskbarFolders.Launcher.Services;

/// <summary>
/// Single source of truth for the cursor position at the moment the launcher process was
/// activated by a taskbar tile click. <see cref="App.OnStartup"/> captures it via
/// <c>GetCursorPos</c> before WPF bootstrap (the only moment the cursor is reliably still
/// over the clicked tile) and seeds it here; <see cref="TaskbarPositionHelper"/> reads it
/// during placement. Existed pre-v0.3 as an inline late <c>GetCursorPos</c> call that ran
/// 100–500 ms after click, by which time the cursor had drifted — the v0.2 "random
/// positioning" bug.
/// </summary>
public interface ICursorAnchor
{
    /// <summary>Stores the cursor location at process activation. Last-write-wins.</summary>
    /// <param name="position">Cursor position in WPF DIPs.</param>
    void Seed(Point position);

    /// <summary>
    /// Returns the seeded position.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Thrown when read before <see cref="Seed"/>.</exception>
    Point Position { get; }
}
