using System.Windows;

namespace TaskbarFolders.Manager.Behaviors;

/// <summary>
/// Attached behavior enabling file drag-and-drop onto UI elements.
/// </summary>
public static class FileDragDropBehavior
{
    /// <summary>
    /// Identifies the IsEnabled attached property.
    /// </summary>
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(FileDragDropBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    /// <summary>
    /// Identifies the DropCommand attached property.
    /// </summary>
    public static readonly DependencyProperty DropCommandProperty =
        DependencyProperty.RegisterAttached(
            "DropCommand",
            typeof(System.Windows.Input.ICommand),
            typeof(FileDragDropBehavior));

    /// <summary>
    /// Gets the IsEnabled value.
    /// </summary>
    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);

    /// <summary>
    /// Sets the IsEnabled value.
    /// </summary>
    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    /// <summary>
    /// Gets the DropCommand value.
    /// </summary>
    public static System.Windows.Input.ICommand? GetDropCommand(DependencyObject obj) =>
        (System.Windows.Input.ICommand?)obj.GetValue(DropCommandProperty);

    /// <summary>
    /// Sets the DropCommand value.
    /// </summary>
    public static void SetDropCommand(DependencyObject obj, System.Windows.Input.ICommand? value) =>
        obj.SetValue(DropCommandProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
            return;

        if ((bool)e.NewValue)
        {
            element.AllowDrop = true;
            element.DragOver += OnDragOver;
            element.Drop += OnDrop;
        }
        else
        {
            element.AllowDrop = false;
            element.DragOver -= OnDragOver;
            element.Drop -= OnDrop;
        }
    }

    private static void OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private static void OnDrop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            return;

        if (e.Data.GetData(DataFormats.FileDrop) is not string[] files || files.Length == 0)
            return;

        var command = GetDropCommand((DependencyObject)sender);
        if (command is not null && command.CanExecute(files))
        {
            command.Execute(files);
        }

        e.Handled = true;
    }
}
