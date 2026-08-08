using System.Windows;
using System.Windows.Media;

namespace Soundpost.App.Controls.Visualizers;

/// <summary>
/// A filled frequency spectrum: a smooth mountain range of the bands, filled with a vertical palette
/// gradient and topped by a bright glowing edge. The curve is over-sampled and neighbourhood-averaged
/// so peaks read as soft hills rather than spikes.
/// </summary>
public sealed class AuroraRenderer : IVisualizerRenderer
{
    public string Name => "Aurora";

    private const int Steps = 140;
    private readonly Point[] _top = new Point[Steps];

    private int _version = -1;
    private Brush _fill = null!;
    private Pen _edge = null!;
    private Pen _glow = null!;

    public void Draw(in VizFrame frame)
    {
        EnsureBrushes(frame.Palette);

        double w = frame.Width, h = frame.Height;
        double maxH = h * 0.82;
        double reach = 0.7 + frame.Sensitivity;

        // Smooth top edge, sampled across the width.
        for (int i = 0; i < Steps; i++)
        {
            double t = i / (double)(Steps - 1);
            double level = VizAudio.BandAt(frame.Bands, t);
            double y = h - Math.Min(maxH, (0.02 + (level * reach)) * maxH);
            _top[i] = new Point(t * w, y);
        }

        DrawingContext dc = frame.Dc;

        // Filled area: down the left edge, across the smoothed top, down the right edge, closed.
        var body = new StreamGeometry();
        using (StreamGeometryContext ctx = body.Open())
        {
            ctx.BeginFigure(new Point(0, h), isFilled: true, isClosed: true);
            ctx.LineTo(_top[0], false, false);
            ctx.PolyLineTo(_top[1..], false, false);
            ctx.LineTo(new Point(w, h), false, false);
        }

        body.Freeze();
        dc.DrawGeometry(_fill, null, body);

        // Bright glowing crest along the top edge.
        var crest = new StreamGeometry();
        using (StreamGeometryContext ctx = crest.Open())
        {
            ctx.BeginFigure(_top[0], isFilled: false, isClosed: false);
            ctx.PolyLineTo(_top[1..], true, true);
        }

        crest.Freeze();

        if (frame.Glow > 0.05)
        {
            dc.PushOpacity(0.35 + (frame.Glow * 0.5));
            dc.DrawGeometry(null, _glow, crest);
            dc.Pop();
        }

        dc.DrawGeometry(null, _edge, crest);
    }

    private void EnsureBrushes(VizPalette palette)
    {
        if (_version == palette.Version)
        {
            return;
        }

        Color c0 = palette.Colors[0], c1 = palette.Colors[1], c2 = palette.Colors[2];

        // Vertical fill: vivid near the baseline, fading up toward the crest.
        var fill = new LinearGradientBrush { StartPoint = new Point(0.5, 0), EndPoint = new Point(0.5, 1) };
        fill.GradientStops.Add(new GradientStop(WithAlpha(Lighten(c0, 0.35), 40), 0));
        fill.GradientStops.Add(new GradientStop(WithAlpha(c2, 150), 0.45));
        fill.GradientStops.Add(new GradientStop(WithAlpha(c1, 220), 1));
        fill.Freeze();
        _fill = fill;

        // Crest: a bright, near-white tint of the top colour so the edge glows like a filament.
        Color crest = Lighten(c0, 0.55);
        var crestBrush = new SolidColorBrush(crest);
        crestBrush.Freeze();
        _edge = VizBrush.FreezePen(crestBrush, 2.0);
        _glow = VizBrush.FreezePen(VizBrush.Fade(crestBrush, 0.5), 9.0);

        _version = palette.Version;
    }

    private static Color Lighten(Color c, double amount) => Color.FromRgb(
        (byte)(c.R + ((255 - c.R) * amount)),
        (byte)(c.G + ((255 - c.G) * amount)),
        (byte)(c.B + ((255 - c.B) * amount)));

    private static Color WithAlpha(Color c, byte a) => Color.FromArgb(a, c.R, c.G, c.B);
}
