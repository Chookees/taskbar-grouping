using System;
using System.Windows;

namespace TaskbarFolders.Launcher.Services;

/// <summary>
/// Default <see cref="ICursorAnchor"/>. Registered as a singleton so the seeded position is
/// shared between <see cref="App.OnStartup"/> (writer) and <see cref="TaskbarPositionHelper"/>
/// (reader). Throws if read before seeded so a future caller that forgets to seed fails fast
/// instead of silently using a default <see cref="Point"/> at (0, 0).
/// </summary>
public sealed class LauncherCursorAnchor : ICursorAnchor
{
    private Point? _position;

    /// <inheritdoc/>
    public void Seed(Point position) => _position = position;

    /// <inheritdoc/>
    public Point Position => _position
        ?? throw new InvalidOperationException("Cursor anchor read before Seed — App.OnStartup must seed it.");
}
