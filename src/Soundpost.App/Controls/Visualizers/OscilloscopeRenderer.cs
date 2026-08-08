using System.Windows;
using System.Windows.Media;

namespace Soundpost.App.Controls.Visualizers;

/// <summary>The raw waveform as a single glowing trace across the screen.</summary>
public sealed class OscilloscopeRenderer : IVisualizerRenderer
{
    public string Name => "Oscilloscope";

    public void Draw(in VizFrame frame)
    {
        float[] wave = frame.Waveform;
        double w = frame.Width, h = frame.Height, cy = h / 2;
        int n = wave.Length;
        double amp = h * 0.42 * (0.6 + frame.Sensitivity);

        var pts = new List<Point>(n);
        for (int i = 0; i < n; i++)
        {
            double x = i / (double)(n - 1) * w;
            pts.Add(new Point(x, cy - (wave[i] * amp)));
        }

        var geo = new StreamGeometry();
        using (StreamGeometryContext ctx = geo.Open())
        {
            ctx.BeginFigure(pts[0], false, false);
            ctx.PolyLineTo(pts.GetRange(1, pts.Count - 1), true, true);
        }

        geo.Freeze();

        DrawingContext dc = frame.Dc;
        dc.PushOpacity(0.95 * (0.5 + (frame.Glow * 0.5)));
        dc.DrawGeometry(null, frame.Palette.GlowPen, geo);
        dc.Pop();
        dc.PushOpacity(0.95);
        dc.DrawGeometry(null, frame.Palette.MainPen, geo);
        dc.Pop();
    }
}
