using System.Runtime.Versioning;
using System.Windows;

namespace TaskbarFolders.Manager.Services;

/// <summary>
/// Default <see cref="IUserConfirmation"/> backed by WPF's <see cref="MessageBox"/>.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class MessageBoxUserConfirmation : IUserConfirmation
{
    /// <inheritdoc/>
    public bool Confirm(string caption, string message)
    {
        var result = MessageBox.Show(
            message,
            caption,
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.No);

        return result == MessageBoxResult.Yes;
    }
}
