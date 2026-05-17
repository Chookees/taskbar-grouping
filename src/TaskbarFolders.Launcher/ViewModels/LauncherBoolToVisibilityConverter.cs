using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TaskbarFolders.Launcher.ViewModels;

/// <summary>True → Visible.</summary>
public sealed class LauncherBoolToVisibilityConverter : IValueConverter
{
    /// <summary>Shared singleton instance.</summary>
    public static readonly LauncherBoolToVisibilityConverter Instance = new();

    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Visibility.Visible : Visibility.Collapsed;

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>True → Collapsed (inverse of <see cref="LauncherBoolToVisibilityConverter"/>).</summary>
public sealed class LauncherInverseBoolToVisibilityConverter : IValueConverter
{
    /// <summary>Shared singleton instance.</summary>
    public static readonly LauncherInverseBoolToVisibilityConverter Instance = new();

    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Visibility.Collapsed : Visibility.Visible;

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Non-empty string → Visible; otherwise Collapsed. Used for the inline error banner.</summary>
public sealed class LauncherNullToCollapsedConverter : IValueConverter
{
    /// <summary>Shared singleton instance.</summary>
    public static readonly LauncherNullToCollapsedConverter Instance = new();

    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string s && !string.IsNullOrWhiteSpace(s) ? Visibility.Visible : Visibility.Collapsed;

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
