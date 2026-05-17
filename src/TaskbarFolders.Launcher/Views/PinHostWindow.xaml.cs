using System.Windows;

namespace TaskbarFolders.Launcher.Views;

/// <summary>
/// Invisible 1×1 off-screen window used as the foreground HWND that
/// <see cref="Windows.UI.Shell.TaskbarManager.RequestPinCurrentAppAsync"/> attaches its
/// system permission dialog to. See <c>PinHostWindow.xaml</c> for the rationale.
/// </summary>
public partial class PinHostWindow : Window
{
    /// <summary>Initializes a new instance.</summary>
    public PinHostWindow()
    {
        InitializeComponent();
    }
}
