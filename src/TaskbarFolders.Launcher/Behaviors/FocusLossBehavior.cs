using System.Windows;

namespace TaskbarFolders.Launcher.Behaviors;

/// <summary>
/// Attached behavior that closes a window when it loses focus.
/// </summary>
public static class FocusLossBehavior
{
    /// <summary>
    /// Identifies the CloseOnFocusLoss attached property.
    /// </summary>
    public static readonly DependencyProperty CloseOnFocusLossProperty =
        DependencyProperty.RegisterAttached(
            "CloseOnFocusLoss",
            typeof(bool),
            typeof(FocusLossBehavior),
            new PropertyMetadata(false, OnCloseOnFocusLossChanged));

    /// <summary>
    /// Gets the CloseOnFocusLoss value.
    /// </summary>
    public static bool GetCloseOnFocusLoss(DependencyObject obj)
        => (bool)obj.GetValue(CloseOnFocusLossProperty);

    /// <summary>
    /// Sets the CloseOnFocusLoss value.
    /// </summary>
    public static void SetCloseOnFocusLoss(DependencyObject obj, bool value)
        => obj.SetValue(CloseOnFocusLossProperty, value);

    private static void OnCloseOnFocusLossChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Window window)
            return;

        if ((bool)e.NewValue)
        {
            window.Deactivated += OnWindowDeactivated;
        }
        else
        {
            window.Deactivated -= OnWindowDeactivated;
        }
    }

    private static void OnWindowDeactivated(object? sender, EventArgs e)
    {
        if (sender is Window window)
            window.Close();
    }
}
