using System.Windows;

namespace Soundpost.App.Controls.Visualizers;

/// <summary>Bands laid out around a circle, each a spoke that grows from the centre with its level.</summary>
public sealed class RadialRenderer : IVisualizerRenderer
{
    public string Name => "Radial";

    public void Draw(in VizFrame frame)
    {
        float[] bands = frame.Bands;
        double w = frame.Width, h = frame.Height;
        var c = new Point(w / 2, h / 2);
        double r0 = Math.Min(w, h) * 0.16;
        double rMax = Math.Min(w, h) * 0.34;
        const int n = 72;

        for (int i = 0; i < n; i++)
        {
            int b = (int)((double)i / n * bands.Length);
            double a = (i / (double)n * Math.PI * 2) + (frame.Time * 0.4);
            double len = r0 + (bands[b] * rMax);
            var p0 = new Point(c.X + (Math.Cos(a) * r0), c.Y + (Math.Sin(a) * r0));
            var p1 = new Point(c.X + (Math.Cos(a) * len), c.Y + (Math.Sin(a) * len));
            frame.Dc.DrawLine(frame.Palette.GlowPen, p0, p1);
            frame.Dc.DrawLine(frame.Palette.MainPen, p0, p1);
        }
    }
}
