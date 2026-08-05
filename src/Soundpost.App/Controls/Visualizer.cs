using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Soundpost.Core.Audio;

namespace Soundpost.App.Controls;

public enum VizStyle
{
    Ribbon,
    Spectrum,
    Oscilloscope,
    Radial,
    Cymatics,
    CustomImage,
}

/// <summary>
/// The live visualizer. Owns a <see cref="LoopbackAnalyzer"/> (starts capture only while visible),
/// smooths the FFT into bands each frame, and draws the selected <see cref="VisualStyle"/> using the
/// current <see cref="Palette"/>. Renderers are intentionally small and self-contained so new styles
/// (including community ones) are easy to add.
/// </summary>
public sealed class Visualizer : FrameworkElement
{
    public static readonly DependencyProperty VisualStyleProperty = DependencyProperty.Register(
        nameof(VisualStyle), typeof(VizStyle), typeof(Visualizer),
        new FrameworkPropertyMetadata(VizStyle.Ribbon, FrameworkPropertyMetadataOptions.AffectsRender));

    public VizStyle VisualStyle { get => (VizStyle)GetValue(VisualStyleProperty); set => SetValue(VisualStyleProperty, value); }

    public double Sensitivity { get; set; } = 0.68;
    public double Smoothing { get; set; } = 0.55;
    public double GlowAmount { get; set; } = 0.72;
    public double Speed { get; set; } = 0.4;

    /// <summary>Image drawn by the Custom Image style; reacts to the audio (pulse + color wash).</summary>
    public ImageSource? CustomImage { get; set; }

    public static readonly string[] PaletteNames = { "Sunset", "Aqua", "Neon", "Ember" };

    private static readonly Color[][] Palettes =
    {
        new[] { Rgb(0xff, 0x7a, 0x1a), Rgb(0xff, 0x3d, 0x7f), Rgb(0xa2, 0x4b, 0xff) }, // Sunset
        new[] { Rgb(0x1a, 0xd1, 0xff), Rgb(0x3d, 0x7f, 0xff), Rgb(0x8b, 0x7b, 0xff) }, // Aqua
        new[] { Rgb(0x39, 0xff, 0x88), Rgb(0x2e, 0xe6, 0xff), Rgb(0xc2, 0x4b, 0xff) }, // Neon
        new[] { Rgb(0xff, 0xd0, 0x8a), Rgb(0xff, 0x8a, 0x3d), Rgb(0xc9, 0x53, 0x1c) }, // Ember
    };

    private const int Bands = 96;
    private readonly LoopbackAnalyzer _analyzer = new();
    private readonly float[] _bands = new float[Bands];
    private readonly float[] _wave = new float[600];
    private double _time;
    private bool _running;

    private int _palette;
    private Pen _mainPen = null!;
    private Pen _glowPen = null!;
    private LinearGradientBrush _barBrush = null!;

    // Cymatics state (allocated lazily).
    private const int CymSize = 200;
    private WriteableBitmap? _cymBmp;
    private byte[]? _cymPixels;
    private double[]? _grain;
    private double[]? _cosNx, _cosMx, _cosMy, _cosNy;
    private double _cymN = 3, _cymM = 2;

    // Custom Image vignette (built once).
    private Brush? _vignette;

    public int PaletteCount => Palettes.Length;

    public int Palette
    {
        get => _palette;
        set
        {
            _palette = Math.Clamp(value, 0, Palettes.Length - 1);
            BuildPalette();
            InvalidateVisual();
        }
    }

    public Visualizer()
    {
        BuildPalette();
        IsVisibleChanged += (_, _) =>
        {
            if (IsVisible)
            {
                Start();
            }
            else
            {
                Stop();
            }
        };
        Unloaded += (_, _) => Stop();
    }

    private void BuildPalette()
    {
        Color[] p = Palettes[_palette];
        var grad = new LinearGradientBrush { StartPoint = new Point(0, 0.5), EndPoint = new Point(1, 0.5) };
        grad.GradientStops.Add(new GradientStop(p[0], 0));
        grad.GradientStops.Add(new GradientStop(p[1], 0.45));
        grad.GradientStops.Add(new GradientStop(p[2], 1));
        grad.Freeze();

        _mainPen = FreezePen(grad, 2.0);
        _glowPen = FreezePen(Fade(grad, 0.35), 6.0);

        _barBrush = new LinearGradientBrush { StartPoint = new Point(0.5, 1), EndPoint = new Point(0.5, 0) };
        _barBrush.GradientStops.Add(new GradientStop(p[0], 0));
        _barBrush.GradientStops.Add(new GradientStop(p[1], 0.6));
        _barBrush.GradientStops.Add(new GradientStop(p[2], 1));
        _barBrush.Freeze();
    }

    private void Start()
    {
        if (_running)
        {
            return;
        }

        _running = true;
        _analyzer.Start();
        CompositionTarget.Rendering += OnFrame;
    }

    private void Stop()
    {
        if (!_running)
        {
            return;
        }

        _running = false;
        CompositionTarget.Rendering -= OnFrame;
        _analyzer.Stop();
    }

    private void OnFrame(object? sender, EventArgs e)
    {
        _time += 0.016 * (0.35 + (Speed * 1.4));
        UpdateBands();
        _analyzer.CopyWaveform(_wave);
        InvalidateVisual();
    }

    private void UpdateBands()
    {
        float[] spec = _analyzer.GetSpectrum();
        int bins = spec.Length;
        double gain = 9 + (Sensitivity * 30);
        double smooth = 0.35 + (Smoothing * 0.6);

        for (int b = 0; b < Bands; b++)
        {
            double t0 = (double)b / Bands;
            double t1 = (double)(b + 1) / Bands;
            int lo = (int)(Math.Pow(t0, 2.2) * (bins - 1));
            int hi = Math.Max(lo + 1, (int)(Math.Pow(t1, 2.2) * (bins - 1)));

            float peak = 0f;
            for (int i = lo; i < hi && i < bins; i++)
            {
                if (spec[i] > peak)
                {
                    peak = spec[i];
                }
            }

            float val = (float)Math.Clamp(Math.Log10(1 + (peak * gain)) * 0.9, 0, 1);
            _bands[b] = (float)((_bands[b] * smooth) + (val * (1 - smooth)));
        }
    }

    protected override void OnRender(DrawingContext dc)
    {
        double w = ActualWidth, h = ActualHeight;
        if (w < 4 || h < 4)
        {
            return;
        }

        switch (VisualStyle)
        {
            case VizStyle.Spectrum: DrawSpectrum(dc, w, h); break;
            case VizStyle.Oscilloscope: DrawScope(dc, w, h); break;
            case VizStyle.Radial: DrawRadial(dc, w, h); break;
            case VizStyle.Cymatics: DrawCymatics(dc, w, h); break;
            case VizStyle.CustomImage: DrawCustomImage(dc, w, h); break;
            default: DrawRibbon(dc, w, h); break;
        }
    }

    private void DrawRibbon(DrawingContext dc, double w, double h)
    {
        double cy = h / 2;
        double maxAmp = h * 0.4;
        const int lines = 9;

        for (int line = 0; line < lines; line++)
        {
            double scale = 1.0 - (line / (double)(lines + 1));
            var top = new List<Point>(Bands);
            var bot = new List<Point>(Bands);
            for (int i = 0; i < Bands; i++)
            {
                double x = i / (double)(Bands - 1) * w;
                double ripple = Math.Sin((i * 0.28) + (_time * 3.0) + line) * (h * 0.012);
                double amp = (_bands[i] * maxAmp * (0.45 + (0.55 * scale))) + ripple;
                top.Add(new Point(x, cy - amp));
                bot.Add(new Point(x, cy + amp));
            }

            double op = 0.28 + (scale * 0.6);
            DrawPolyline(dc, top, op);
            DrawPolyline(dc, bot, op);
        }
    }

    private void DrawSpectrum(DrawingContext dc, double w, double h)
    {
        int bars = 64;
        double gap = 3;
        double bw = (w - ((bars - 1) * gap)) / bars;
        for (int i = 0; i < bars; i++)
        {
            int b = (int)((double)i / bars * Bands);
            double bh = Math.Max(2, _bands[b] * h * 0.92);
            double x = i * (bw + gap);
            dc.DrawRoundedRectangle(_barBrush, null, new Rect(x, h - bh, bw, bh), 2, 2);
        }
    }

    private void DrawScope(DrawingContext dc, double w, double h)
    {
        double cy = h / 2;
        int n = _wave.Length;
        var pts = new List<Point>(n);
        double amp = h * 0.42 * (0.6 + Sensitivity);
        for (int i = 0; i < n; i++)
        {
            double x = i / (double)(n - 1) * w;
            pts.Add(new Point(x, cy - (_wave[i] * amp)));
        }

        DrawPolyline(dc, pts, 0.95);
    }

    private void DrawRadial(DrawingContext dc, double w, double h)
    {
        var c = new Point(w / 2, h / 2);
        double r0 = Math.Min(w, h) * 0.16;
        double rMax = Math.Min(w, h) * 0.34;
        int n = 72;
        for (int i = 0; i < n; i++)
        {
            int b = (int)((double)i / n * Bands);
            double a = (i / (double)n * Math.PI * 2) + (_time * 0.4);
            double len = r0 + (_bands[b] * rMax);
            var p0 = new Point(c.X + (Math.Cos(a) * r0), c.Y + (Math.Sin(a) * r0));
            var p1 = new Point(c.X + (Math.Cos(a) * len), c.Y + (Math.Sin(a) * len));
            dc.DrawLine(_glowPen, p0, p1);
            dc.DrawLine(_mainPen, p0, p1);
        }
    }

    // Cymatic (Chladni) sand plate: nodal lines of a vibrating square plate. The mode numbers
    // (n, m) are driven by where the music's energy sits; louder audio thickens the sand.
    private void DrawCymatics(DrawingContext dc, double w, double h)
    {
        const int s = CymSize;
        if (_cymBmp is null)
        {
            _cymBmp = new WriteableBitmap(s, s, 96, 96, PixelFormats.Bgra32, null);
            _cymPixels = new byte[s * s * 4];
            _cosNx = new double[s];
            _cosMx = new double[s];
            _cosMy = new double[s];
            _cosNy = new double[s];
            _grain = new double[s * s];
            var rnd = new Random(1234);
            for (int k = 0; k < _grain.Length; k++)
            {
                _grain[k] = 0.45 + (rnd.NextDouble() * 0.55);
            }
        }

        int half = Bands / 2;
        double nTarget = 2 + (ArgMax(0, half) / (double)Math.Max(1, half) * 5.0);
        double mTarget = 2 + ((ArgMax(half, Bands) - half) / (double)Math.Max(1, Bands - half) * 5.0);
        _cymN += (nTarget - _cymN) * 0.05;
        _cymM += (mTarget - _cymM) * 0.05;

        double energy = Energy();
        double eps = 0.03 + (energy * 0.16 * (0.4 + Sensitivity));

        for (int i = 0; i < s; i++)
        {
            double x = i / (double)(s - 1);
            _cosNx![i] = Math.Cos(_cymN * Math.PI * x);
            _cosMx![i] = Math.Cos(_cymM * Math.PI * x);
        }

        for (int j = 0; j < s; j++)
        {
            double y = j / (double)(s - 1);
            _cosMy![j] = Math.Cos(_cymM * Math.PI * y);
            _cosNy![j] = Math.Cos(_cymN * Math.PI * y);
        }

        byte[] px = _cymPixels!;
        int stride = s * 4;
        for (int j = 0; j < s; j++)
        {
            double cmy = _cosMy![j], cny = _cosNy![j];
            int row = j * stride;
            int grow = j * s;
            for (int i = 0; i < s; i++)
            {
                double f = (_cosNx![i] * cmy) - (_cosMx![i] * cny);
                double af = Math.Abs(f);
                double a = af < eps ? 1 - (af / eps) : 0;
                a *= _grain![grow + i];
                int o = row + (i * 4);
                px[o] = (byte)(214 * a);       // B
                px[o + 1] = (byte)(238 * a);   // G
                px[o + 2] = (byte)(255 * a);   // R
                px[o + 3] = 255;               // A
            }
        }

        _cymBmp.WritePixels(new Int32Rect(0, 0, s, s), px, stride, 0);
        dc.DrawImage(_cymBmp, new Rect(0, 0, w, h));
    }

    private void DrawCustomImage(DrawingContext dc, double w, double h)
    {
        if (CustomImage is null)
        {
            return; // Empty state ("drop an image") is drawn by the view's overlay.
        }

        double iw = CustomImage.Width, ih = CustomImage.Height;
        if (iw <= 0 || ih <= 0)
        {
            return;
        }

        // Cover-fit, then pulse the zoom with the bass so the picture breathes with the beat.
        double bass = BandAvg(0, 12);
        double energy = Energy();
        double scale = 1 + (bass * 0.16 * (0.5 + Sensitivity));
        double fit = Math.Max(w / iw, h / ih) * scale;
        double dw = iw * fit, dh = ih * fit;
        dc.DrawImage(CustomImage, new Rect((w - dw) / 2, (h - dh) / 2, dw, dh));

        // Palette wash that swells with the overall energy.
        Color c = Palettes[_palette][1];
        byte alpha = (byte)Math.Clamp(energy * 110 * (0.35 + GlowAmount), 0, 135);
        if (alpha > 0)
        {
            dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(alpha, c.R, c.G, c.B)), null, new Rect(0, 0, w, h));
        }

        // Vignette so the picture settles inside the console frame.
        dc.DrawRectangle(_vignette ??= BuildVignette(), null, new Rect(0, 0, w, h));
    }

    private void DrawPolyline(DrawingContext dc, List<Point> pts, double opacity)
    {
        var geo = new StreamGeometry();
        using (StreamGeometryContext ctx = geo.Open())
        {
            ctx.BeginFigure(pts[0], false, false);
            ctx.PolyLineTo(pts.GetRange(1, pts.Count - 1), true, true);
        }

        geo.Freeze();
        dc.PushOpacity(opacity * (0.5 + (GlowAmount * 0.5)));
        dc.DrawGeometry(null, _glowPen, geo);
        dc.Pop();
        dc.PushOpacity(opacity);
        dc.DrawGeometry(null, _mainPen, geo);
        dc.Pop();
    }

    private int ArgMax(int lo, int hi)
    {
        int index = lo;
        float max = -1f;
        for (int i = lo; i < hi && i < Bands; i++)
        {
            if (_bands[i] > max)
            {
                max = _bands[i];
                index = i;
            }
        }

        return index;
    }

    private double Energy()
    {
        double sum = 0;
        for (int i = 0; i < Bands; i++)
        {
            sum += _bands[i];
        }

        return sum / Bands;
    }

    private double BandAvg(int lo, int hi)
    {
        double sum = 0;
        int n = 0;
        for (int i = lo; i < hi && i < Bands; i++)
        {
            sum += _bands[i];
            n++;
        }

        return n == 0 ? 0 : sum / n;
    }

    private static Color Rgb(byte r, byte g, byte b) => Color.FromRgb(r, g, b);

    private static Brush Fade(Brush source, double opacity)
    {
        Brush b = source.Clone();
        b.Opacity = opacity;
        b.Freeze();
        return b;
    }

    private static Pen FreezePen(Brush brush, double thickness)
    {
        var pen = new Pen(brush, thickness) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round, LineJoin = PenLineJoin.Round };
        pen.Freeze();
        return pen;
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
