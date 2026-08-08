using System.Windows;

namespace Soundpost.App.Controls.Visualizers;

/// <summary>Classic bar spectrum: one rounded bar per frequency band, height driven by its level.</summary>
public sealed class SpectrumRenderer : IVisualizerRenderer
{
    public string Name => "Spectrum";

    public void Draw(in VizFrame frame)
    {
        float[] bands = frame.Bands;
        double w = frame.Width, h = frame.Height;

        const int bars = 64;
        const double gap = 3;
        double bw = (w - ((bars - 1) * gap)) / bars;

        for (int i = 0; i < bars; i++)
        {
            // Mirrored so bass sits in the centre and the bars are symmetric, not left-loaded.
            double level = VizAudio.MirroredBandAt(bands, i / (double)(bars - 1));
            double bh = Math.Max(2, level * h * 0.92);
            double x = i * (bw + gap);
            frame.Dc.DrawRoundedRectangle(frame.Palette.BarBrush, null, new Rect(x, h - bh, bw, bh), 2, 2);
        }
    }
}
