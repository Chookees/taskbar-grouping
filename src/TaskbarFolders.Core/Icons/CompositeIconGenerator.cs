using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TaskbarFolders.Core.Icons;

/// <summary>
/// Generates 2x2 composite icons from multiple source icons.
/// </summary>
public sealed class CompositeIconGenerator : ICompositeIconGenerator
{
    private static readonly Color DefaultBackground = Color.FromArgb(230, 240, 240, 240);

    /// <inheritdoc />
    public BitmapSource GenerateComposite(IReadOnlyList<BitmapSource> icons, int outputSize = 256)
    {
        ArgumentNullException.ThrowIfNull(icons);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(outputSize);

        int cellSize = outputSize / 2;
        int padding = outputSize / 16;
        int iconSize = cellSize - padding;

        var visual = new DrawingVisual();
        using (DrawingContext ctx = visual.RenderOpen())
        {
            DrawBackground(ctx, outputSize);
            DrawIcons(ctx, icons, cellSize, padding, iconSize);
        }

        var bitmap = new RenderTargetBitmap(outputSize, outputSize, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();

        return bitmap;
    }

    private static void DrawBackground(DrawingContext ctx, int size)
    {
        var backgroundBrush = new SolidColorBrush(DefaultBackground);
        backgroundBrush.Freeze();

        double cornerRadius = size / 8.0;
        var rect = new Rect(0, 0, size, size);
        var geometry = new RectangleGeometry(rect, cornerRadius, cornerRadius);
        geometry.Freeze();

        ctx.DrawGeometry(backgroundBrush, null, geometry);
    }

    private static void DrawIcons(DrawingContext ctx, IReadOnlyList<BitmapSource> icons,
        int cellSize, int padding, int iconSize)
    {
        var positions = GetPositions(icons.Count, cellSize, padding);

        for (int i = 0; i < Math.Min(icons.Count, 4); i++)
        {
            var (x, y) = positions[i];
            ctx.DrawImage(icons[i], new Rect(x, y, iconSize, iconSize));
        }
    }

    private static (int x, int y)[] GetPositions(int count, int cellSize, int padding)
    {
        int halfPad = padding / 2;

        return count switch
        {
            1 => [(cellSize / 2, cellSize / 2)],
            2 => [(halfPad, cellSize / 2), (cellSize, cellSize / 2)],
            3 => [(halfPad, halfPad), (cellSize, halfPad), (cellSize / 2, cellSize)],
            _ => [(halfPad, halfPad), (cellSize, halfPad), (halfPad, cellSize), (cellSize, cellSize)],
        };
    }
}
