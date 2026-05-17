using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TaskbarFolders.Core.Icons;

/// <summary>
/// Default <see cref="ICompositeIconGenerator"/>. Renders up to four source icons into a
/// single square <see cref="BitmapSource"/> using WPF's <see cref="DrawingVisual"/> +
/// <see cref="RenderTargetBitmap"/> pipeline. Layouts follow the user-guide spec:
/// 1 = centred, 2 = side by side, 3 = iOS-style two-top-one-bottom, 4+ = 2×2 grid
/// of the first four icons.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class CompositeIconGenerator : ICompositeIconGenerator
{
    /// <summary>Padding around the composite as a fraction of the output size.</summary>
    public const double PaddingFraction = 0.08;

    /// <summary>Gap between adjacent tiles as a fraction of the output size.</summary>
    public const double GapFraction = 0.04;

    /// <summary>Maximum number of source icons rendered into the composite.</summary>
    public const int MaxTiles = 4;

    /// <inheritdoc/>
    public BitmapSource GenerateComposite(IReadOnlyList<BitmapSource> icons, int outputSize = 256)
    {
        ArgumentNullException.ThrowIfNull(icons);
        if (icons.Count == 0)
        {
            throw new ArgumentException("At least one source icon is required.", nameof(icons));
        }

        if (outputSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(outputSize), "Output size must be positive.");
        }

        var rects = ComputeTileRects(Math.Min(icons.Count, MaxTiles), outputSize);

        var visual = new DrawingVisual();
        using (var ctx = visual.RenderOpen())
        {
            for (var i = 0; i < rects.Count; i++)
            {
                DrawIconInTile(ctx, icons[i], rects[i]);
            }
        }

        var bitmap = new RenderTargetBitmap(outputSize, outputSize, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
    }

    /// <summary>
    /// Computes the destination rectangles for a given tile count and output size.
    /// Public for unit testing so geometry can be verified without spinning up WPF rendering.
    /// </summary>
    /// <param name="count">Number of tiles (must be in [1..<see cref="MaxTiles"/>]).</param>
    /// <param name="outputSize">Size of the square composite.</param>
    public static IReadOnlyList<Rect> ComputeTileRects(int count, int outputSize)
    {
        if (count < 1 || count > MaxTiles)
        {
            throw new ArgumentOutOfRangeException(nameof(count), $"Count must be in [1..{MaxTiles}].");
        }

        if (outputSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(outputSize), "Output size must be positive.");
        }

        var padding = outputSize * PaddingFraction;
        var gap = outputSize * GapFraction;
        var fullSpan = outputSize - 2 * padding;

        return count switch
        {
            1 => SingleTile(outputSize),
            2 => TwoTiles(padding, gap, fullSpan, outputSize),
            3 => ThreeTiles(padding, gap, fullSpan),
            4 => FourTiles(padding, gap, fullSpan),
            _ => throw new InvalidOperationException("Unreachable — count was bounds-checked above."),
        };
    }

    private static Rect[] SingleTile(int outputSize)
    {
        // 80% of the output, centred — gives the icon enough breathing room without
        // looking lost in a transparent canvas.
        var size = outputSize * (1 - 2 * PaddingFraction);
        var offset = (outputSize - size) / 2;
        return [new Rect(offset, offset, size, size)];
    }

    private static Rect[] TwoTiles(double padding, double gap, double fullSpan, int outputSize)
    {
        var tile = (fullSpan - gap) / 2;
        var top = (outputSize - tile) / 2;
        return
        [
            new Rect(padding, top, tile, tile),
            new Rect(padding + tile + gap, top, tile, tile),
        ];
    }

    private static Rect[] ThreeTiles(double padding, double gap, double fullSpan)
    {
        // iOS-style: two on top, one bottom-centre. All three tiles are the same size
        // so they read as a coherent group.
        var tile = (fullSpan - gap) / 2;
        var bottomRow = padding + tile + gap;
        var bottomLeft = padding + (fullSpan - tile) / 2;
        return
        [
            new Rect(padding, padding, tile, tile),
            new Rect(padding + tile + gap, padding, tile, tile),
            new Rect(bottomLeft, bottomRow, tile, tile),
        ];
    }

    private static Rect[] FourTiles(double padding, double gap, double fullSpan)
    {
        var tile = (fullSpan - gap) / 2;
        return
        [
            new Rect(padding, padding, tile, tile),
            new Rect(padding + tile + gap, padding, tile, tile),
            new Rect(padding, padding + tile + gap, tile, tile),
            new Rect(padding + tile + gap, padding + tile + gap, tile, tile),
        ];
    }

    private static void DrawIconInTile(DrawingContext ctx, BitmapSource icon, Rect tile)
    {
        // Preserve aspect ratio. Square icons (the common case) fill the tile exactly.
        var iconAspect = (double)icon.PixelWidth / icon.PixelHeight;
        var tileAspect = tile.Width / tile.Height;

        double drawWidth;
        double drawHeight;
        if (iconAspect > tileAspect)
        {
            drawWidth = tile.Width;
            drawHeight = tile.Width / iconAspect;
        }
        else
        {
            drawHeight = tile.Height;
            drawWidth = tile.Height * iconAspect;
        }

        var x = tile.Left + (tile.Width - drawWidth) / 2;
        var y = tile.Top + (tile.Height - drawHeight) / 2;

        ctx.DrawImage(icon, new Rect(x, y, drawWidth, drawHeight));
    }
}
