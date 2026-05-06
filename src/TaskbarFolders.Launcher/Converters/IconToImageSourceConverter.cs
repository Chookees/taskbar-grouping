using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace TaskbarFolders.Launcher.Converters;

/// <summary>
/// Converts a BitmapSource to an ImageSource, providing a fallback for null icons.
/// </summary>
[ValueConversion(typeof(BitmapSource), typeof(BitmapSource))]
public sealed class IconToImageSourceConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value;

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
