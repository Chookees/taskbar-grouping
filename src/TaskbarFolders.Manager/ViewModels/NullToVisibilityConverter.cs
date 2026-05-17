using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TaskbarFolders.Manager.ViewModels;

/// <summary>
/// Returns <see cref="Visibility.Visible"/> when the bound value is <see langword="null"/>,
/// otherwise <see cref="Visibility.Collapsed"/>. Used for empty-state placeholders.
/// Exposes a static <see cref="Instance"/> so XAML can use <c>{x:Static}</c> without resource declarations.
/// </summary>
public sealed class NullToVisibilityConverter : IValueConverter
{
    /// <summary>Shared singleton instance.</summary>
    public static readonly NullToVisibilityConverter Instance = new();

    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is null ? Visibility.Visible : Visibility.Collapsed;

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
