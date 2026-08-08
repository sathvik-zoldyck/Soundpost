using System.Windows;
using System.Windows.Media;

namespace Soundpost.App.Controls.Visualizers;

/// <summary>
/// A lens of nested strands: amplitude is shaped by an envelope that falls to zero at both edges, so
/// the ribbon converges to a point left and right and swells in the middle. Strands are hairlines
/// rather than thick glowing bands — that keeps the curves legible and the rasterised area (the real
/// cost of this style) small enough to hold 60fps.
/// </summary>
public sealed class RibbonRenderer : IVisualizerRenderer
{
    public string Name => "Ribbon";

    private const int Strands = 9;
    private const int Steps = 96;

    private readonly double[] _envelope = BuildEnvelope();
    private readonly double[] _x = new double[Steps];
    private readonly double[] _level = new double[Steps];
    private readonly double[] _sinA = new double[Steps];
    private readonly double[] _cosA = new double[Steps];

    private readonly StreamGeometry[] _geometry = new StreamGeometry[3];
    private readonly StreamGeometryContext[] _context = new StreamGeometryContext[3];

    // Opacity tiers, each a pre-faded frozen pen. Baking alpha into the pen means the loop never
    // calls PushOpacity, which would otherwise force a composition layer per strand every frame.
    private int _penVersion = -1;
    private Pen[] _tierPens = System.Array.Empty<Pen>();
    private Pen _glowPen = null!;

    public void Draw(in VizFrame frame)
    {
        EnsurePens(frame.Palette);

        double w = frame.Width, h = frame.Height;
        double cy = h / 2;
        double maxAmp = h * 0.44;
        double reach = 0.5 + frame.Sensitivity;

        // Everything that depends only on the step (not the strand) is computed once per frame.
        for (int i = 0; i < Steps; i++)
        {
            double t = i / (double)(Steps - 1);
            _x[i] = t * w;
            _level[i] = VizAudio.BandAt(frame.Bands, t);
            double a = (t * 5.5) + (frame.Time * 2.2);
            _sinA[i] = Math.Sin(a);
            _cosA[i] = Math.Cos(a);
        }

        int tiers = _tierPens.Length;
        for (int i = 0; i < tiers; i++)
        {
            _geometry[i] = new StreamGeometry();
            _context[i] = _geometry[i].Open();
        }

        for (int s = 1; s <= Strands; s++)
        {
            double scale = s / (double)Strands;
            int tier = Math.Min(tiers - 1, (int)(scale * tiers * 0.999));

            // sin(A + B) expanded, so the per-strand phase costs two multiplies, not a fresh Sin.
            double phase = s * 0.55;
            double sinB = Math.Sin(phase);
            double cosB = Math.Cos(phase);

            for (int side = 0; side < 2; side++)
            {
                double sign = side == 0 ? -1 : 1;
                StreamGeometryContext ctx = _context[tier];

                for (int i = 0; i < Steps; i++)
                {
                    double wobble = ((_sinA[i] * cosB) + (_cosA[i] * sinB)) * 0.05;
                    double amp = maxAmp * scale * _envelope[i] * (0.14 + ((_level[i] + wobble) * reach));
                    var p = new Point(_x[i], cy + (sign * amp));

                    if (i == 0)
                    {
                        ctx.BeginFigure(p, false, false);
                    }
                    else
                    {
                        ctx.LineTo(p, true, false);
                    }
                }
            }
        }

        for (int i = 0; i < tiers; i++)
        {
            _context[i].Close();
            _geometry[i].Freeze();
        }

        DrawingContext dc = frame.Dc;

        // Bloom rides on the outer tier only — the inner strands stay crisp.
        if (frame.Glow > 0.05)
        {
            dc.PushOpacity(frame.Glow * 0.45);
            dc.DrawGeometry(null, _glowPen, _geometry[tiers - 1]);
            dc.Pop();
        }

        for (int i = 0; i < tiers; i++)
        {
            dc.DrawGeometry(null, _tierPens[i], _geometry[i]);
        }
    }

    private void EnsurePens(VizPalette palette)
    {
        if (_penVersion == palette.Version)
        {
            return;
        }

        Brush grad = VizBrush.HorizontalGradient(palette.Colors);
        _tierPens = new[]
        {
            VizBrush.FreezePen(VizBrush.Fade(grad, 0.30), 1.1),
            VizBrush.FreezePen(VizBrush.Fade(grad, 0.55), 1.2),
            VizBrush.FreezePen(VizBrush.Fade(grad, 0.90), 1.3),
        };
        _glowPen = palette.GlowPen;
        _penVersion = palette.Version;
    }

    // The lens taper: zero at both edges, widest in the middle. Never changes.
    private static double[] BuildEnvelope()
    {
        var envelope = new double[Steps];
        for (int i = 0; i < Steps; i++)
        {
            envelope[i] = Math.Pow(Math.Sin(i / (double)(Steps - 1) * Math.PI), 0.7);
        }

        return envelope;
    }
}
