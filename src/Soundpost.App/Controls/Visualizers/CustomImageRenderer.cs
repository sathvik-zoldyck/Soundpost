using System.Windows;
using System.Windows.Media;

namespace Soundpost.App.Controls.Visualizers;

/// <summary>
/// Draws a user-supplied image that breathes with the music: cover-fit, a bass-driven zoom pulse, a
/// palette wash that swells with energy, and a vignette so it settles inside the console frame. The
/// empty state ("drop an image") is drawn by the view overlay, so this simply no-ops until an image
/// arrives.
/// </summary>
public sealed class CustomImageRenderer : IVisualizerRenderer, IRequiresImage
{
    public string Name => "Custom Image";

    private Brush? _vignette;

    public void Draw(in VizFrame frame)
    {
        ImageSource? image = frame.Image;
        if (image is null)
        {
            return;
        }

        double iw = image.Width, ih = image.Height;
        if (iw <= 0 || ih <= 0)
        {
            return;
        }

        double w = frame.Width, h = frame.Height;
        DrawingContext dc = frame.Dc;

        // Cover-fit, then pulse the zoom with the bass so the picture breathes with the beat.
        double bass = VizAudio.BandAvg(frame.Bands, 0, 12);
        double energy = VizAudio.Energy(frame.Bands);
        double scale = 1 + (bass * 0.16 * (0.5 + frame.Sensitivity));
        double fit = Math.Max(w / iw, h / ih) * scale;
        double dw = iw * fit, dh = ih * fit;
        dc.DrawImage(image, new Rect((w - dw) / 2, (h - dh) / 2, dw, dh));

        // Palette wash that swells with the overall energy.
        Color c = frame.Palette.Colors[1];
        byte alpha = (byte)Math.Clamp(energy * 110 * (0.35 + frame.Glow), 0, 135);
        if (alpha > 0)
        {
            dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(alpha, c.R, c.G, c.B)), null, new Rect(0, 0, w, h));
        }

        dc.DrawRectangle(_vignette ??= BuildVignette(), null, new Rect(0, 0, w, h));
    }

    private static Brush BuildVignette()
    {
        var brush = new RadialGradientBrush
        {
            GradientOrigin = new Point(0.5, 0.5),
            Center = new Point(0.5, 0.5),
            RadiusX = 0.78,
            RadiusY = 0.78,
        };
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 5, 6, 9), 0.55));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(160, 5, 6, 9), 1));
        brush.Freeze();
        return brush;
    }
}
