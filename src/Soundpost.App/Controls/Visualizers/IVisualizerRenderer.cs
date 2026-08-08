using System.Windows;
using System.Windows.Media;

namespace Soundpost.App.Controls.Visualizers;

/// <summary>
/// A visualiser style: one method that paints a frame from smoothed audio. This is the whole
/// contract a community style implements — write a class, add it to the registry in
/// <see cref="Visualizer"/>, and it appears in the style bar. Keep <see cref="Draw"/> allocation-light;
/// it runs on every frame.
/// </summary>
public interface IVisualizerRenderer
{
    /// <summary>Shown on the style pill and the on-screen HUD. Keep it short.</summary>
    string Name { get; }

    /// <summary>Paint one frame. Everything you need is on <paramref name="frame"/>.</summary>
    void Draw(in VizFrame frame);
}

/// <summary>
/// Marks a renderer that draws a user-supplied image (so the view offers a picker / drop zone and
/// shows an empty state until one is chosen). The image arrives on <see cref="VizFrame.Image"/>.
/// </summary>
public interface IRequiresImage
{
}

/// <summary>
/// Everything a renderer gets for one frame: the surface to draw on, its size, the smoothed audio,
/// the live control values, and the active palette. A cheap readonly struct — passed by <c>in</c>.
/// </summary>
public readonly struct VizFrame
{
    public required DrawingContext Dc { get; init; }
    public required double Width { get; init; }
    public required double Height { get; init; }

    /// <summary>Smoothed FFT bands, 0..1, low frequency first.</summary>
    public required float[] Bands { get; init; }

    /// <summary>Recent mono waveform, -1..1.</summary>
    public required float[] Waveform { get; init; }

    /// <summary>Seconds-ish clock that advances with the Speed knob; use it for motion.</summary>
    public required double Time { get; init; }

    public required double Sensitivity { get; init; }
    public required double Smoothing { get; init; }
    public required double Glow { get; init; }
    public required double Speed { get; init; }

    /// <summary>The user's chosen colours plus ready-made pens/brushes. Respect it.</summary>
    public required VizPalette Palette { get; init; }

    /// <summary>The image for <see cref="IRequiresImage"/> styles; null until the user picks one.</summary>
    public ImageSource? Image { get; init; }
}

/// <summary>
/// The active colour scheme, resolved into ready-to-use frozen brushes and pens so renderers don't
/// rebuild them each frame. <see cref="Version"/> bumps whenever the palette changes, letting a
/// renderer cache derived resources and rebuild only when it must.
/// </summary>
public sealed class VizPalette
{
    public int Version { get; }

    /// <summary>Three accent colours, low → high.</summary>
    public IReadOnlyList<Color> Colors { get; }

    /// <summary>Horizontal low→high gradient (frozen).</summary>
    public Brush Gradient { get; }

    /// <summary>2px gradient stroke.</summary>
    public Pen MainPen { get; }

    /// <summary>Thick translucent stroke for the glow pass.</summary>
    public Pen GlowPen { get; }

    /// <summary>Vertical gradient for bars.</summary>
    public Brush BarBrush { get; }

    public VizPalette(int version, Color[] colors)
    {
        Version = version;
        Colors = colors;

        Gradient = VizBrush.HorizontalGradient(colors);
        MainPen = VizBrush.FreezePen(Gradient, 2.0);
        GlowPen = VizBrush.FreezePen(VizBrush.Fade(Gradient, 0.35), 6.0);

        var bar = new LinearGradientBrush { StartPoint = new Point(0.5, 1), EndPoint = new Point(0.5, 0) };
        bar.GradientStops.Add(new GradientStop(colors[0], 0));
        bar.GradientStops.Add(new GradientStop(colors[1], 0.6));
        bar.GradientStops.Add(new GradientStop(colors[2], 1));
        bar.Freeze();
        BarBrush = bar;
    }
}

/// <summary>Small brush/pen helpers shared by the palette and the renderers.</summary>
public static class VizBrush
{
    public static Brush HorizontalGradient(IReadOnlyList<Color> colors)
    {
        var grad = new LinearGradientBrush { StartPoint = new Point(0, 0.5), EndPoint = new Point(1, 0.5) };
        grad.GradientStops.Add(new GradientStop(colors[0], 0));
        grad.GradientStops.Add(new GradientStop(colors[1], 0.45));
        grad.GradientStops.Add(new GradientStop(colors[2], 1));
        grad.Freeze();
        return grad;
    }

    public static Brush Fade(Brush source, double opacity)
    {
        Brush b = source.Clone();
        b.Opacity = opacity;
        b.Freeze();
        return b;
    }

    public static Pen FreezePen(Brush brush, double thickness)
    {
        var pen = new Pen(brush, thickness)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round,
            LineJoin = PenLineJoin.Round,
        };
        pen.Freeze();
        return pen;
    }
}

/// <summary>Read-only helpers over the smoothed band array, for renderers that want shaped energy.</summary>
public static class VizAudio
{
    /// <summary>Mean of all bands (overall loudness), 0..1.</summary>
    public static double Energy(float[] bands)
    {
        double sum = 0;
        for (int i = 0; i < bands.Length; i++)
        {
            sum += bands[i];
        }

        return bands.Length == 0 ? 0 : sum / bands.Length;
    }

    /// <summary>Mean of bands in [lo, hi).</summary>
    public static double BandAvg(float[] bands, int lo, int hi)
    {
        double sum = 0;
        int n = 0;
        for (int i = lo; i < hi && i < bands.Length; i++)
        {
            sum += bands[i];
            n++;
        }

        return n == 0 ? 0 : sum / n;
    }

    /// <summary>
    /// Band energy for a mirrored layout: t runs 0..1 across the width, bass sits in the centre and
    /// the range fans out symmetrically to both edges — so a frequency-driven shape reads as a
    /// centred mountain instead of piling up on one side. Starts just above 0 Hz so the dead DC bin
    /// doesn't notch the centre.
    /// </summary>
    public static double MirroredBandAt(float[] bands, double t)
    {
        double p = Math.Abs(t - 0.5) * 2.0; // 0 at centre, 1 at the edges
        return BandAt(bands, 0.05 + (p * 0.9));
    }

    /// <summary>Band energy at normalised position t, averaged over a small neighbourhood (smooth).</summary>
    public static double BandAt(float[] bands, double t)
    {
        int count = bands.Length;
        double pos = Math.Clamp(t, 0, 1) * (count - 1);
        int centre = (int)pos;

        double sum = 0;
        int n = 0;
        for (int k = centre - 3; k <= centre + 3; k++)
        {
            if (k >= 0 && k < count)
            {
                sum += bands[k];
                n++;
            }
        }

        return n == 0 ? 0 : sum / n;
    }

    /// <summary>Index of the loudest band in [lo, hi).</summary>
    public static int ArgMax(float[] bands, int lo, int hi)
    {
        int index = lo;
        float max = -1f;
        for (int i = lo; i < hi && i < bands.Length; i++)
        {
            if (bands[i] > max)
            {
                max = bands[i];
                index = i;
            }
        }

        return index;
    }
}
