using System.Windows;
using System.Windows.Media;

namespace Soundpost.App.Controls.Visualizers;

/// <summary>
/// A range of ridgelines — the mirrored band energy drawn as a few filled mountain layers that scroll
/// at different speeds, so the shape reads as depth instead of a single bar graph. Bass sits in the
/// centre (via <see cref="VizAudio.MirroredBandAt"/>); each layer is a closed area filled with a faded
/// vertical gradient and topped by a bright rim, the nearest layer catching the glow. Geometry is
/// rebuilt each frame like <see cref="RibbonRenderer"/>, but stays cheap: a short polyline per layer
/// and pre-faded frozen brushes/pens keyed to the palette version.
/// </summary>
public sealed class RidgeRenderer : IVisualizerRenderer
{
    public string Name => "Ridge";

    private const int Layers = 3;
    private const int Steps = 128;

    private readonly Point[] _top = new Point[Steps]; // the ridgeline, reused by the fill and the rim

    private int _penVersion = -1;
    private Brush[] _fills = System.Array.Empty<Brush>();
    private Pen[] _rimPens = System.Array.Empty<Pen>();
    private Pen _glowPen = null!;

    public void Draw(in VizFrame frame)
    {
        EnsureResources(frame.Palette);

        double w = frame.Width, h = frame.Height;
        double reach = 0.35 + frame.Sensitivity;
        DrawingContext dc = frame.Dc;

        // Farthest layer first, so nearer ridges overlap it. depth 0..1 grows toward the viewer.
        for (int layer = 0; layer < Layers; layer++)
        {
            double depth = (layer + 1) / (double)Layers;
            double maxAmp = h * (0.16 + (depth * 0.42));
            double baseline = h * (0.60 + (depth * 0.34)); // nearer ridges sit lower on the screen
            double scroll = frame.Time * (0.15 + (depth * 0.5)); // parallax: nearer scrolls faster
            double phase = layer * 1.7;

            for (int i = 0; i < Steps; i++)
            {
                double t = i / (double)(Steps - 1);
                double level = VizAudio.MirroredBandAt(frame.Bands, t);
                // A little travelling relief so a steady tone still undulates rather than sitting flat.
                double relief = 0.12 * (0.5 + (0.5 * Math.Sin((t * 9.0) + scroll + phase)));
                double amp = maxAmp * ((level * reach) + relief);
                _top[i] = new Point(t * w, baseline - amp);
            }

            var fill = new StreamGeometry();
            using (StreamGeometryContext ctx = fill.Open())
            {
                ctx.BeginFigure(new Point(0, baseline), true, true);
                for (int i = 0; i < Steps; i++)
                {
                    ctx.LineTo(_top[i], true, false);
                }

                ctx.LineTo(new Point(w, baseline), true, false);
            }

            fill.Freeze();

            var rim = new StreamGeometry();
            using (StreamGeometryContext ctx = rim.Open())
            {
                ctx.BeginFigure(_top[0], false, false);
                for (int i = 1; i < Steps; i++)
                {
                    ctx.LineTo(_top[i], true, false);
                }
            }

            rim.Freeze();

            dc.DrawGeometry(_fills[layer], null, fill);

            // Glow rides the nearest ridge only, so the far layers stay crisp and cheap.
            if (layer == Layers - 1 && frame.Glow > 0.05)
            {
                dc.PushOpacity(frame.Glow * 0.5);
                dc.DrawGeometry(null, _glowPen, rim);
                dc.Pop();
            }

            dc.DrawGeometry(null, _rimPens[layer], rim);
        }
    }

    private void EnsureResources(VizPalette palette)
    {
        if (_penVersion == palette.Version)
        {
            return;
        }

        IReadOnlyList<Color> colors = palette.Colors;
        double[] fillAlpha = { 0.22, 0.40, 0.66 };
        double[] rimAlpha = { 0.40, 0.70, 1.00 };
        double[] rimWidth = { 1.0, 1.4, 1.8 };

        _fills = new Brush[Layers];
        _rimPens = new Pen[Layers];
        Brush rimGradient = VizBrush.HorizontalGradient(colors);

        for (int layer = 0; layer < Layers; layer++)
        {
            var fill = new LinearGradientBrush { StartPoint = new Point(0.5, 0), EndPoint = new Point(0.5, 1) };
            fill.GradientStops.Add(new GradientStop(colors[2], 0));   // bright at the ridge crest
            fill.GradientStops.Add(new GradientStop(colors[1], 0.5));
            fill.GradientStops.Add(new GradientStop(colors[0], 1));   // darker toward the base
            fill.Opacity = fillAlpha[layer];
            fill.Freeze();

            _fills[layer] = fill;
            _rimPens[layer] = VizBrush.FreezePen(VizBrush.Fade(rimGradient, rimAlpha[layer]), rimWidth[layer]);
        }

        _glowPen = palette.GlowPen;
        _penVersion = palette.Version;
    }
}
