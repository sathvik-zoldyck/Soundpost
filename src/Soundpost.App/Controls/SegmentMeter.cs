using System.Windows;
using System.Windows.Media;

namespace Soundpost.App.Controls;

/// <summary>
/// A segmented LED-style peak meter (like a channel meter on a mixing desk). Draws bottom-up
/// when vertical, left-to-right when horizontal, coloring segments green → amber → red.
/// Driven by <see cref="Level"/> (0–1), which the render loop updates from live peaks.
/// </summary>
public sealed class SegmentMeter : FrameworkElement
{
    public static readonly DependencyProperty LevelProperty = DependencyProperty.Register(
        nameof(Level), typeof(double), typeof(SegmentMeter),
        new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SegmentsProperty = DependencyProperty.Register(
        nameof(Segments), typeof(int), typeof(SegmentMeter),
        new FrameworkPropertyMetadata(12, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty HorizontalProperty = DependencyProperty.Register(
        nameof(Horizontal), typeof(bool), typeof(SegmentMeter),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public double Level { get => (double)GetValue(LevelProperty); set => SetValue(LevelProperty, value); }

    public int Segments { get => (int)GetValue(SegmentsProperty); set => SetValue(SegmentsProperty, value); }

    public bool Horizontal { get => (bool)GetValue(HorizontalProperty); set => SetValue(HorizontalProperty, value); }

    private static readonly Brush Off = Freeze(0x17, 0x1b, 0x22);
    private static readonly Brush Low = Freeze(0x37, 0xe0, 0xa0);
    private static readonly Brush Mid = Freeze(0xff, 0xc2, 0x4b);
    private static readonly Brush Hot = Freeze(0xff, 0x54, 0x68);

    private static SolidColorBrush Freeze(byte r, byte g, byte b)
    {
        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }

    protected override void OnRender(DrawingContext dc)
    {
        int n = Math.Max(1, Segments);
        double level = Math.Clamp(Level, 0, 1);
        int lit = (int)Math.Round(level * n);
        const double gap = 3;

        if (Horizontal)
        {
            double segW = (ActualWidth - ((n - 1) * gap)) / n;
            for (int i = 0; i < n; i++)
            {
                double x = i * (segW + gap);
                dc.DrawRoundedRectangle(BrushFor(i, lit, n), null, new Rect(x, 0, segW, ActualHeight), 1.5, 1.5);
            }
        }
        else
        {
            double segH = (ActualHeight - ((n - 1) * gap)) / n;
            for (int i = 0; i < n; i++)
            {
                double y = ActualHeight - ((i + 1) * segH) - (i * gap);
                dc.DrawRoundedRectangle(BrushFor(i, lit, n), null, new Rect(0, y, ActualWidth, segH), 1.5, 1.5);
            }
        }
    }

    private static Brush BrushFor(int i, int lit, int n)
    {
        if (i >= lit)
        {
            return Off;
        }

        double frac = (double)i / n;
        return frac > 0.86 ? Hot : frac > 0.66 ? Mid : Low;
    }
}
