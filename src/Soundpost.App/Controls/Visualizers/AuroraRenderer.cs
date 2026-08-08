using System.Windows;
using System.Windows.Media;

namespace Soundpost.App.Controls.Visualizers;

/// <summary>
/// A filled frequency spectrum drawn as a range of coloured mountains: the fill runs a horizontal
/// palette gradient across the width, so each peak carries its own hue, and a bright vivid crest
/// glows along the top edge. The curve is over-sampled and box-smoothed into soft hills. Cheap by
/// design — two stroked/​filled geometries a frame, no per-pixel work or frame feedback.
/// </summary>
public sealed class AuroraRenderer : IVisualizerRenderer
{
    public string Name => "Aurora";

    private const int Steps = 128;
    private readonly Point[] _top = new Point[Steps];
    private readonly double[] _level = new double[Steps];
    private readonly double[] _smooth = new double[Steps];

    // Auto-gain reference: a decaying peak so the ridge always rises to fill the height, however
    // loud or quiet the source is. Seeded low so it settles quickly on first sound.
    private double _reference = 0.25;

    private int _version = -1;
    private Brush _fill = null!;   // horizontal palette gradient, translucent
    private Pen _crest = null!;    // bright horizontal gradient, crisp
    private Pen _glow = null!;     // same, soft and wide
    private Pen _base = null!;     // bright neon floor line

    public void Draw(in VizFrame frame)
    {
        EnsureBrushes(frame.Palette);

        double w = frame.Width, h = frame.Height;
        double maxH = h * 0.9;
        double center = (Steps - 1) / 2.0;

        // Mirror the spectrum around the centre: bass (the loud part) sits in the middle and the
        // range fans out symmetrically to both edges, so the shape is a centred mountain rather
        // than everything piled up on the left.
        double frameMax = 0;
        for (int i = 0; i < Steps; i++)
        {
            double p = Math.Abs(i - center) / center; // 0 at centre, 1 at the edges
            // Start a little above 0 Hz: the very lowest bin carries no musical energy, so mapping
            // the exact centre to it would always cut a notch there. Begin at real bass instead.
            _level[i] = VizAudio.BandAt(frame.Bands, 0.05 + (p * 0.9));
            if (_level[i] > frameMax)
            {
                frameMax = _level[i];
            }
        }

        // Auto-gain: track a decaying peak and scale to it, so the ridge always reaches toward the
        // top on the loudest recent moment. A floor keeps near-silence from amplifying into noise.
        _reference = Math.Max(frameMax, _reference * 0.96);
        _reference = Math.Max(_reference, 0.06);
        double gain = (0.75 + (frame.Sensitivity * 0.7)) / _reference;

        // One box-smoothing pass so the ridge reads as soft hills, not a jagged spectrum.
        for (int i = 0; i < Steps; i++)
        {
            double sum = 0;
            int n = 0;
            for (int k = i - 3; k <= i + 3; k++)
            {
                if (k >= 0 && k < Steps)
                {
                    sum += _level[k];
                    n++;
                }
            }

            _smooth[i] = sum / n;
        }

        for (int i = 0; i < Steps; i++)
        {
            double t = i / (double)(Steps - 1);
            double amp = Math.Clamp(_smooth[i] * gain, 0, 1);
            double y = h - Math.Min(maxH, (0.02 + (amp * 0.98)) * maxH);
            _top[i] = new Point(t * w, y);
        }

        DrawingContext dc = frame.Dc;

        // Filled body — horizontal gradient, so the mountains are individually coloured.
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

        // Bright glowing crest along the ridge.
        var crest = new StreamGeometry();
        using (StreamGeometryContext ctx = crest.Open())
        {
            ctx.BeginFigure(_top[0], isFilled: false, isClosed: false);
            ctx.PolyLineTo(_top[1..], true, true);
        }

        crest.Freeze();

        if (frame.Glow > 0.05)
        {
            dc.PushOpacity(0.4 + (frame.Glow * 0.5));
            dc.DrawGeometry(null, _glow, crest);
            dc.Pop();
        }

        dc.DrawGeometry(null, _crest, crest);

        // A vivid neon floor, like the reference's bright baseline.
        dc.DrawLine(_base, new Point(0, h - 1.2), new Point(w, h - 1.2));
    }

    private void EnsureBrushes(VizPalette palette)
    {
        if (_version == palette.Version)
        {
            return;
        }

        Color c0 = palette.Colors[0], c1 = palette.Colors[1], c2 = palette.Colors[2];

        // Translucent horizontal gradient for the fill — colours vary across the width and the video
        // shows through in Clear mode.
        _fill = HorizontalGradient(WithAlpha(c0, 165), WithAlpha(c1, 150), WithAlpha(c2, 165));

        // Crest/glow use *boosted* colours — brightened by scaling RGB, which keeps the hue vivid
        // instead of washing to a muddy pastel the way blending toward white does.
        // Glow opacity is baked into the pen (rather than PushOpacity per frame), and its width is
        // kept modest — a wide translucent stroke over a long ridge is the one real cost here.
        Brush bright = HorizontalGradient(Boost(c0), Boost(c1), Boost(c2));
        _crest = VizBrush.FreezePen(bright, 2.4);
        _glow = VizBrush.FreezePen(VizBrush.Fade(bright, 0.5), 8.5);
        _base = VizBrush.FreezePen(bright, 3.0);

        _version = palette.Version;
    }

    private static Brush HorizontalGradient(Color a, Color b, Color c)
    {
        var grad = new LinearGradientBrush { StartPoint = new Point(0, 0.5), EndPoint = new Point(1, 0.5) };
        grad.GradientStops.Add(new GradientStop(a, 0));
        grad.GradientStops.Add(new GradientStop(b, 0.5));
        grad.GradientStops.Add(new GradientStop(c, 1));
        grad.Freeze();
        return grad;
    }

    // Brighten while preserving hue/saturation: scale each channel up and clamp, rather than mixing
    // toward white (which desaturates — the source of the "brown" edge).
    private static Color Boost(Color c, double f = 1.5) => Color.FromRgb(
        (byte)Math.Min(255, c.R * f),
        (byte)Math.Min(255, c.G * f),
        (byte)Math.Min(255, c.B * f));

    private static Color WithAlpha(Color c, byte a) => Color.FromArgb(a, c.R, c.G, c.B);
}
