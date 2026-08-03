using System.Windows;
using System.Windows.Media;
using Soundpost.Core.Audio;

namespace Soundpost.App.Controls;

public enum VizStyle
{
    Ribbon,
    Spectrum,
    Oscilloscope,
    Radial,
    Aurora,
}

/// <summary>
/// The live visualizer. Owns a <see cref="LoopbackAnalyzer"/> (starts capture only while visible),
/// smooths the FFT into bands each frame, and draws the selected <see cref="VisualStyle"/> using the
/// current <see cref="Palette"/>. Glow is faked with a thick translucent pass under a bright pass.
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
            case VizStyle.Aurora: DrawAurora(dc, w, h); break;
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

    private void DrawAurora(DrawingContext dc, double w, double h)
    {
        // Soft, slow, overlapping colored clouds that swell with the music — no hard lines.
        double cy = h / 2;
        Color[] p = Palettes[_palette];
        const int blobs = 7;
        byte alpha = (byte)(70 + (GlowAmount * 90));

        for (int i = 0; i < blobs; i++)
        {
            double t = (i + 0.5) / blobs;
            int band = (int)(t * Bands * 0.8);
            double energy = _bands[band];
            double bx = t * w;
            double by = cy + (Math.Sin((_time * 0.7) + (i * 1.3)) * h * 0.16);
            double rr = (0.14 + (energy * 0.95 * (0.6 + Sensitivity))) * h;
            Color col = p[i % p.Length];

            var brush = new RadialGradientBrush();
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(alpha, col.R, col.G, col.B), 0));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, col.R, col.G, col.B), 1));
            brush.Freeze();
            dc.DrawEllipse(brush, null, new Point(bx, by), rr, rr * 0.72);
        }
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
}
